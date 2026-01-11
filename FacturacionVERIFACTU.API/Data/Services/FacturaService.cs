// API/Data/Services/FacturaService.cs
using API.Data.Entities;
using FacturacionVERIFACTU.API.Data.Entities;
using FacturacionVERIFACTU.API.DTOs;
using FacturacionVERIFACTU.API.Services;
using Microsoft.EntityFrameworkCore;

namespace FacturacionVERIFACTU.API.Data.Services
{
    public interface IFacturaService
    {
        Task<FacturaResponseDto> CrearFacturaAsync(int tenantId, FacturaCreateDto dto);
        Task<FacturaResponseDto> ActualizarFacturaAsync(int tenantId, int id, FacturaUpdateDto dto);
        Task<FacturaResponseDto> MarcarComoPagadaAsync(int tenantId, int id, MarcarComoPagadaDto dto);
        Task<FacturaResponseDto> AnularFacturaAsync(int tenantId, int id, AnularFacturaDto dto);
        Task<FacturaResponseDto> ObtenerPorIdAsync(int tenantId, int id);
        Task<List<FacturaResponseDto>> ObtenerTodosAsync(int tenantId, int? id, string? estado, DateTime? fechaDesde, DateTime? fechaHasta);
        Task<bool> EliminarAsync(int tenantId, int id);
        Task<FacturaResponseDto> ConvertirDesdePresupuestoAsync(int tenantId, int id, ConvertirPresupuestoAFacturaDto dto);
        Task<FacturaResponseDto> ConvertirDesdeAlbaranAsync(int tenantId, int id, ConvertirAlbaranesAFacturaDto dto);
    }

    public class FacturaService : IFacturaService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISerieNumeracionService _numeracionService;
        private readonly VERIFACTUService _verifactuService;
        private readonly AEATClient _verifactuHttpClient;
        private readonly ILogger<FacturaService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public FacturaService(
            ApplicationDbContext context,
            ISerieNumeracionService numeracionService,
            VERIFACTUService verifactuService,
            AEATClient verifactuHttpClient,
            ILogger<FacturaService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _context = context;
            _numeracionService = numeracionService;
            _verifactuService = verifactuService;
            _verifactuHttpClient = verifactuHttpClient;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        // ====================================================================
        // CREAR FACTURA
        // ====================================================================
        // API/Services/FacturaService.cs - ACTUALIZAR CrearFacturaAsync

        public async Task<FacturaResponseDto> CrearFacturaAsync(int tenantId, FacturaCreateDto dto)
        {
            // Validar y cargar cliente CON configuración fiscal
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == dto.ClienteId && c.TenantId == tenantId);

            if (cliente == null)
                throw new InvalidOperationException($"Cliente {dto.ClienteId} no encontrado");

            // Validar productos
            var lineasProductoIds = dto.Lineas
                .Where(l => l.ProductoId.HasValue)
                .Select(l => l.ProductoId.Value)
                .Distinct()
                .ToList();

            Dictionary<int, Producto> productosDict = new();

            if (lineasProductoIds.Any())
            {
                var productos = await _context.Productos
                    .Where(p => p.TenantId == tenantId && lineasProductoIds.Contains(p.Id))
                    .ToListAsync();

                if (productos.Count != lineasProductoIds.Count)
                {
                    var idFaltante = lineasProductoIds.First(id => !productos.Any(p => p.Id == id));
                    throw new InvalidOperationException($"El producto con ID {idFaltante} no fue encontrado.");
                }

                productosDict = productos.ToDictionary(p => p.Id);
            }

            // Validar serie
            var serie = await _context.SeriesNumeracion
                .FirstOrDefaultAsync(s => s.Id == dto.SerieId && s.TenantId == tenantId);

            if (serie == null)
                throw new InvalidOperationException($"Serie {dto.SerieId} no encontrada");

            if (serie.Bloqueada)
                throw new InvalidOperationException("La serie está bloqueada. No se pueden crear facturas");

            // Obtener siguiente número
            var ejercicio = dto.FechaEmision?.Year ?? DateTime.UtcNow.Year;
            var (numeroCompleto, numero) = await _numeracionService
                .ObtenerSiguienteNumeroAsync(tenantId, serie.Codigo, ejercicio, "FACTURA");

            // ⭐ APLICAR CONFIGURACIÓN FISCAL DEL CLIENTE
            var porcentajeRetencion = dto.PorcentajeRetencion
                ?? cliente.PorcentajeRetencionDefecto;

            // Crear factura
            var factura = new Factura
            {
                TenantId = tenantId,
                ClienteId = dto.ClienteId,
                SerieId = dto.SerieId,
                Numero = numeroCompleto,
                Ejercicio = ejercicio,
                FechaEmision = (dto.FechaEmision ?? DateTime.UtcNow).ToUniversalTime(),
                Estado = "Emitida",
                Observaciones = dto.Observaciones,
                TipoFacturaVERIFACTU = dto.TipoFacturaVERIFACTU,
                NumeroFacturaRectificada = dto.NumeroFacturaRectificada,
                TipoRectificacion = dto.TipoRectificacion,

                PorcentajeRetencion = porcentajeRetencion, // ⭐ DESDE CLIENTE
                Bloqueada = false,
                ActualizadoEn = DateTime.UtcNow
            };

            // ⭐ AGREGAR LÍNEAS CON CONFIGURACIÓN FISCAL AUTOMÁTICA
            int orden = 1;
            foreach (var lineaDto in dto.Lineas)
            {
                decimal iva = lineaDto.IVA ?? 0;
                decimal recargo = 0;

                // Si la línea tiene producto, obtener su configuración fiscal
                if (lineaDto.ProductoId.HasValue && productosDict.ContainsKey(lineaDto.ProductoId.Value))
                {
                    var producto = productosDict[lineaDto.ProductoId.Value];

                    // ⭐ APLICAR IVA DEL PRODUCTO si no se especificó
                    if (lineaDto.IVA == 0 || !lineaDto.IVA.HasValue)
                    {
                        iva = producto.IVADefecto;
                    }

                    // ⭐ APLICAR RECARGO SOLO SI EL CLIENTE LO REQUIERE
                    if (cliente.RegimenRecargoEquivalencia)
                    {
                        // Si se especificó recargo en DTO, usarlo; sino usar el del producto
                        recargo = lineaDto.RecargoEquivalencia
                            ?? producto.RecargoEquivalenciaDefecto ?? 0M;

                        _logger.LogInformation(
                            "Aplicado recargo equivalencia {Recargo}% a producto {Producto} para cliente {Cliente}",
                            recargo, producto.Descripcion, cliente.Nombre);
                    }
                }
                else
                {
                    // Línea sin producto: usar valores del DTO o calcular
                    iva = lineaDto.IVA ?? 21m;

                    if (cliente.RegimenRecargoEquivalencia)
                    {
                        recargo = lineaDto.RecargoEquivalencia
                            ?? FiscalConfiguracionService.CalcularRecargoEquivalencia(iva);
                    }
                }

                var linea = new LineaFactura
                {
                    Orden = orden++,
                    Descripcion = lineaDto.Descripcion,
                    Cantidad = lineaDto.Cantidad,
                    PrecioUnitario = lineaDto.PrecioUnitario,
                    IVA = iva,                              // ⭐ APLICADO AUTOMÁTICAMENTE
                    RecargoEquivalencia = recargo,          // ⭐ APLICADO AUTOMÁTICAMENTE
                    ProductoId = lineaDto.ProductoId,
                    Importe = Math.Round(lineaDto.Cantidad * lineaDto.PrecioUnitario, 2)
                };

                factura.Lineas.Add(linea);
            }

            // Calcular totales
            CalcularTotalesFactura(factura);

            // Log de configuración fiscal aplicada
            if (cliente.RegimenRecargoEquivalencia)
            {
                _logger.LogInformation(
                    "Factura {Numero}: Aplicado recargo equivalencia. Total recargo: {TotalRecargo}€",
                    numeroCompleto, factura.TotalRecargo);
            }

            if (porcentajeRetencion > 0)
            {
                _logger.LogInformation(
                    "Factura {Numero}: Aplicada retención {Porcentaje}%. Cuota: {Cuota}€",
                    numeroCompleto, porcentajeRetencion, factura.CuotaRetencion);
            }

            // Obtener hash de la factura anterior
            var facturaAnterior = await _context.Facturas
                .Where(f => f.SerieId == dto.SerieId && f.TenantId == tenantId)
                .OrderByDescending(f => f.FechaEmision)
                .ThenByDescending(f => f.Id)
                .FirstOrDefaultAsync();

            factura.HuellaAnterior = facturaAnterior?.Huella;

            // Aplicar VERIFACTU
            await AplicarVERIFACTUAsync(factura);

            _context.Facturas.Add(factura);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Creada factura {Numero} para tenant {TenantId}", numeroCompleto, tenantId);

            // Enviar a AEAT en background
            _ = Task.Run(async () =>
            {
                try
                {
                    await EnviarFacturaAEATAsync(factura.Id, tenantId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar factura {FacturaId} a AEAT", factura.Id);
                }
            });

            return await MapearAResponseDto(factura);
        }


        // ====================================================================
        // ACTUALIZAR FACTURA
        // ====================================================================
        // API/Services/FacturaService.cs - ActualizarFacturaAsync COMPLETO

        public async Task<FacturaResponseDto> ActualizarFacturaAsync(int tenantId, int id, FacturaUpdateDto dto)
        {
            var factura = await _context.Facturas
                .Include(f => f.Lineas)
                .Include(f => f.Serie)
                .Include(f => f.Cliente) // ⭐ IMPORTANTE: Incluir cliente
                .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == tenantId);

            if (factura == null)
                throw new InvalidOperationException($"Factura {id} no encontrada");

            // Validaciones de bloqueo
            if (factura.Bloqueada)
                throw new InvalidOperationException(
                    "No se puede modificar una factura de un ejercicio cerrado. " +
                    "Debe reabrir el ejercicio primero.");

            if (factura.Estado != "Emitida")
                throw new InvalidOperationException($"No se puede modificar una factura en estado {factura.Estado}");

            if (factura.EnviadaVERIFACTU)
                throw new InvalidOperationException($"No se puede modificar una factura enviada a AEAT");

            if (factura.Serie.Bloqueada)
                throw new InvalidOperationException("No se puede modificar una factura de un ejercicio cerrado");

            // ⭐ SI SE CAMBIÓ EL CLIENTE, RECARGAR CONFIGURACIÓN FISCAL
            Cliente clienteActual = factura.Cliente;

            if (dto.ClienteId != factura.ClienteId)
            {
                clienteActual = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.Id == dto.ClienteId && c.TenantId == tenantId);

                if (clienteActual == null)
                    throw new InvalidOperationException($"Cliente {dto.ClienteId} no encontrado");

                factura.ClienteId = dto.ClienteId;

                // ⭐ RECALCULAR RETENCIÓN CON NUEVO CLIENTE
                factura.PorcentajeRetencion = dto.PorcentajeRetencion
                    ?? clienteActual.PorcentajeRetencionDefecto;

                _logger.LogInformation(
                    "Factura {Id}: Cliente cambiado a {Cliente}. Nueva retención: {Retencion}%",
                    id, clienteActual.Nombre, factura.PorcentajeRetencion);
            }
            else
            {
                // ⭐ ACTUALIZAR RETENCIÓN SI SE ESPECIFICÓ EN EL DTO
                if (dto.PorcentajeRetencion.HasValue)
                {
                    factura.PorcentajeRetencion = dto.PorcentajeRetencion.Value;
                }
            }

            // Validar productos
            var lineasProductoIds = dto.Lineas
                .Where(l => l.ProductoId.HasValue)
                .Select(l => l.ProductoId.Value)
                .Distinct()
                .ToList();

            Dictionary<int, Producto> productosDict = new();

            if (lineasProductoIds.Any())
            {
                var productos = await _context.Productos
                    .Where(p => p.TenantId == tenantId && lineasProductoIds.Contains(p.Id))
                    .ToListAsync();

                if (productos.Count != lineasProductoIds.Count)
                {
                    var idFaltante = lineasProductoIds.First(id => !productos.Any(p => p.Id == id));
                    throw new InvalidOperationException($"El producto con ID {idFaltante} no fue encontrado.");
                }

                productosDict = productos.ToDictionary(p => p.Id);
            }

            // Actualizar datos básicos
            factura.FechaEmision = (dto.FechaEmision ?? factura.FechaEmision).ToUniversalTime();
            factura.Observaciones = dto.Observaciones;
            factura.ActualizadoEn = DateTime.UtcNow;

            // Actualización inteligente de líneas
            var lineasDtoIds = dto.Lineas
                .Where(l => l.Id.HasValue)
                .Select(l => l.Id.Value)
                .ToList();

            var lineasAEliminar = factura.Lineas
                .Where(l => !lineasDtoIds.Contains(l.Id))
                .ToList();

            foreach (var lineaAEliminar in lineasAEliminar)
            {
                factura.Lineas.Remove(lineaAEliminar);
                _context.LineasFacturas.Remove(lineaAEliminar);
            }

            int orden = 1;
            foreach (var lineaDto in dto.Lineas)
            {
                // ⭐ CALCULAR IVA Y RECARGO AUTOMÁTICAMENTE
                decimal iva = lineaDto.IVA ?? 21m;
                decimal recargo = 0;

                // Si tiene producto, usar su configuración
                if (lineaDto.ProductoId.HasValue && productosDict.ContainsKey(lineaDto.ProductoId.Value))
                {
                    var producto = productosDict[lineaDto.ProductoId.Value];

                    // Si no se especificó IVA, usar el del producto
                    if (!lineaDto.IVA.HasValue || lineaDto.IVA == 0)
                    {
                        iva = producto.IVADefecto;
                    }

                    // ⭐ APLICAR RECARGO SEGÚN CONFIGURACIÓN DEL CLIENTE
                    if (clienteActual.RegimenRecargoEquivalencia)
                    {
                        recargo = lineaDto.RecargoEquivalencia
                            ?? producto.RecargoEquivalenciaDefecto ?? 0m;
                    }
                }
                else
                {
                    // Sin producto: usar valores del DTO o calcular
                    iva = lineaDto.IVA ?? 21m;

                    if (clienteActual.RegimenRecargoEquivalencia)
                    {
                        recargo = lineaDto.RecargoEquivalencia
                            ?? FiscalConfiguracionService.CalcularRecargoEquivalencia(iva);
                    }
                }

                if (lineaDto.Id.HasValue && lineaDto.Id.Value > 0)
                {
                    // Actualizar línea existente
                    var lineaExistente = factura.Lineas
                        .FirstOrDefault(l => l.Id == lineaDto.Id.Value);

                    if (lineaExistente != null)
                    {
                        lineaExistente.Orden = orden++;
                        lineaExistente.Descripcion = lineaDto.Descripcion;
                        lineaExistente.Cantidad = lineaDto.Cantidad;
                        lineaExistente.PrecioUnitario = lineaDto.PrecioUnitario;
                        lineaExistente.IVA = iva;                          // ⭐ RECALCULADO
                        lineaExistente.RecargoEquivalencia = recargo;       // ⭐ RECALCULADO
                        lineaExistente.ProductoId = lineaDto.ProductoId;
                        lineaExistente.Importe = Math.Round(lineaDto.Cantidad * lineaDto.PrecioUnitario, 2);
                    }
                }
                else
                {
                    // Crear línea nueva
                    var lineaNueva = new LineaFactura
                    {
                        FacturaId = factura.Id,
                        Orden = orden++,
                        Descripcion = lineaDto.Descripcion,
                        Cantidad = lineaDto.Cantidad,
                        PrecioUnitario = lineaDto.PrecioUnitario,
                        IVA = iva,                                  // ⭐ CALCULADO
                        RecargoEquivalencia = recargo,              // ⭐ CALCULADO
                        ProductoId = lineaDto.ProductoId,
                        Importe = Math.Round(lineaDto.Cantidad * lineaDto.PrecioUnitario, 2)
                    };

                    factura.Lineas.Add(lineaNueva);
                }
            }

            // Recalcular totales
            CalcularTotalesFactura(factura);

            // Log de cambios fiscales
            _logger.LogInformation(
                "Factura {Numero} actualizada. Cliente en recargo: {Recargo}. Retención: {Retencion}%",
                factura.Numero, clienteActual.RegimenRecargoEquivalencia, factura.PorcentajeRetencion);

            // Regenerar hash VERIFACTU
            await AplicarVERIFACTUAsync(factura);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Actualizada factura {Numero} para tenant {TenantID}", factura.Numero, tenantId);

            return await MapearAResponseDto(factura);
        }

        // ====================================================================
        // MARCAR COMO PAGADA
        // ====================================================================
        public async Task<FacturaResponseDto> MarcarComoPagadaAsync(int tenantId, int id, MarcarComoPagadaDto dto)
        {
            var factura = await _context.Facturas
                .Include(f => f.Lineas)
                .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == tenantId);

            if (factura == null)
                throw new InvalidOperationException($"Factura {id} no encontrada");

            if (factura.Estado == "Anulada")
                throw new InvalidOperationException("No se puede marcar como pagada una factura anulada");

            if (factura.Estado == "Pagada")
                throw new InvalidOperationException("La factura ya está marcada como pagada");

            // Validar importe
            if (dto.Importe != factura.Total)
            {
                _logger.LogWarning(
                    "Factura {FacturaId}: Importe pagado ({Importe}) diferente del total({Total})",
                    id, dto.Importe, factura.Total);
            }

            var estadoAnterior = factura.Estado;
            factura.Estado = "Pagada";
            factura.ActualizadoEn = DateTime.UtcNow; // ⭐ NUEVO

            // Agregar información del pago para observaciones
            if (!string.IsNullOrEmpty(dto.Observaciones) || !string.IsNullOrEmpty(dto.FormaPago))
            {
                factura.Observaciones = (factura.Observaciones ?? "") +
                    $"\n[PAGADA] {dto.FechaPago:yyyy-MM-dd}: {dto.FormaPago ?? "Sin especificar"}" +
                    (string.IsNullOrEmpty(dto.Observaciones) ? "" : $" - {dto.Observaciones}");
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cambiado estado de factura {Numero} de {EstadoAnterior} a Pagada",
                factura.Numero, estadoAnterior);

            return await MapearAResponseDto(factura);
        }


        // ====================================================================
        // ANULAR FACTURA
        // ====================================================================
        public async Task<FacturaResponseDto> AnularFacturaAsync(int tenantId, int id, AnularFacturaDto dto)
        {
            var factura = await _context.Facturas
                .Include(f => f.Lineas)
                .Include(f => f.Serie)
                .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == tenantId);

            if (factura == null)
                throw new InvalidOperationException($"Factura {id} no encontrada");

            // ⭐ VALIDAR BLOQUEO POR CIERRE
            if (factura.Bloqueada)
                throw new InvalidOperationException(
                    "No se puede anular una factura de un ejercicio cerrado. " +
                    "Debe crear una factura rectificativa.");

            if (factura.Serie.Bloqueada)
                throw new InvalidOperationException("No se puede anular una factura de un ejercicio cerrado. " +
                    "Debe crear una factura rectificativa");

            if (factura.Estado == "Anulada")
                throw new InvalidOperationException("La factura ya está anulada");

            if (factura.EnviadaVERIFACTU)
            {
                throw new InvalidOperationException("La factura ya fue enviada a AEAT. " +
                    "Debe crear una factura rectificativa en lugar de anularla");
            }

            var estadoAnterior = factura.Estado;
            factura.Estado = "Anulada";
            factura.ActualizadoEn = DateTime.UtcNow; // ⭐ NUEVO
            factura.Observaciones = (factura.Observaciones ?? "") +
                $"\n[ANULADA] {DateTime.UtcNow:yyyy-MM-dd}: {dto.Motivo}";

            await _context.SaveChangesAsync();

            _logger.LogWarning("Anulada factura {Numero}. Motivo: {Motivo}",
                factura.Numero, dto.Motivo);

            return await MapearAResponseDto(factura);
        }


        // ====================================================================
        // OBTENER POR ID
        // ====================================================================
        public async Task<FacturaResponseDto> ObtenerPorIdAsync(int tenantId, int id)
        {
            var factura = await _context.Facturas
                .Include(f => f.Lineas)
                .Include(f => f.Cliente)
                .Include(f => f.Serie)
                .Include(f => f.Albaranes)
                .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == tenantId);

            if (factura == null) return null;

            return await MapearAResponseDto(factura);
        }


        // ====================================================================
        // OBTENER TODOS
        // ====================================================================
        public async Task<List<FacturaResponseDto>> ObtenerTodosAsync(
            int tenantId,
            int? clienteId = null,
            string? estado = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null)
        {
            var query = _context.Facturas
                .Include(f => f.Lineas)
                .Include(f => f.Cliente)
                .Include(f => f.Serie)
                .Where(f => f.TenantId == tenantId);

            if (clienteId.HasValue)
                query = query.Where(f => f.ClienteId == clienteId.Value);

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(f => f.Estado == estado);

            if (fechaDesde.HasValue)
                query = query.Where(f => f.FechaEmision >= fechaDesde.Value.ToUniversalTime());

            if (fechaHasta.HasValue)
                query = query.Where(f => f.FechaEmision <= fechaHasta.Value.ToUniversalTime());

            var facturas = await query
                .OrderByDescending(f => f.FechaEmision)
                .ToListAsync();

            var result = new List<FacturaResponseDto>();
            foreach (var factura in facturas)
            {
                result.Add(await MapearAResponseDto(factura));
            }

            return result;
        }


        // ====================================================================
        // ELIMINAR
        // ====================================================================
        public async Task<bool> EliminarAsync(int tenantId, int id)
        {
            var factura = await _context.Facturas
                .Include(f => f.Serie)
                .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == tenantId);

            if (factura == null) return false;

            // ⭐ VALIDAR BLOQUEO
            if (factura.Bloqueada)
                throw new InvalidOperationException(
                    "No se puede eliminar una factura de un ejercicio cerrado.");

            // Solo se pueden eliminar facturas en estado emitida y no enviada
            if (factura.Estado != "Emitida")
                throw new InvalidOperationException($"No se puede eliminar una factura en estado {factura.Estado}.");

            if (factura.EnviadaVERIFACTU)
                throw new InvalidOperationException("No se puede eliminar una factura ya enviada a AEAT");

            _context.Facturas.Remove(factura);
            await _context.SaveChangesAsync();

            _logger.LogWarning("Eliminada factura {numero} del tenant {TenantId}",
                factura.Numero, tenantId);

            return true;
        }


        // ====================================================================
        // CONVERTIR DESDE PRESUPUESTO
        // ====================================================================
        // API/Services/FacturaService.cs - ConvertirDesdePresupuestoAsync

        public async Task<FacturaResponseDto> ConvertirDesdePresupuestoAsync(
            int tenantId, int presupuestoId, ConvertirPresupuestoAFacturaDto dto)
        {
            // Obtener presupuesto CON CLIENTE
            var presupuesto = await _context.Presupuestos
                .Include(p => p.Lineas)
                .Include(p => p.Cliente) // ⭐ IMPORTANTE
                .FirstOrDefaultAsync(p => p.Id == presupuestoId && p.TenantId == tenantId);

            if (presupuesto == null)
                throw new InvalidOperationException($"Presupuesto {presupuestoId} no encontrado");

            if (presupuesto.Estado != "Aceptado")
                throw new InvalidOperationException($"Solo se pueden convertir presupuestos en estado aceptado");

            if (presupuesto.FacturaId != null)
                throw new InvalidOperationException("Este presupuesto ya tiene una factura asociada");

            // Validar serie
            var serie = await _context.SeriesNumeracion
                .FirstOrDefaultAsync(s => s.Id == dto.SerieId && s.TenantId == tenantId);

            if (serie == null)
                throw new InvalidOperationException($"Serie {dto.SerieId} no encontrada");

            if (serie.Bloqueada)
                throw new InvalidOperationException("La serie está bloqueada");

            // ⭐ CARGAR CONFIGURACIÓN FISCAL DEL CLIENTE
            var cliente = presupuesto.Cliente;

            // Obtener siguiente número
            var ejercicio = dto.FechaEmision?.Year ?? DateTime.UtcNow.Year;
            var (numeroCompleto, numero) = await _numeracionService
                .ObtenerSiguienteNumeroAsync(tenantId, serie.Codigo, ejercicio, "FACTURA");

            // Crear factura
            var factura = new Factura
            {
                TenantId = tenantId,
                ClienteId = presupuesto.ClienteId,
                SerieId = serie.Id,
                Numero = numeroCompleto,
                Ejercicio = ejercicio,
                FechaEmision = (dto.FechaEmision ?? DateTime.UtcNow).ToUniversalTime(),
                Estado = "Emitida",
                Observaciones = dto.Observaciones ?? presupuesto.Observaciones,
                TipoFacturaVERIFACTU = dto.TipoFacturaVERIFACTU,
                // ⭐ MANTENER O APLICAR RETENCIÓN
                PorcentajeRetencion = dto.PorcentajeRetencion
                     ?? presupuesto.PorcentajeRetencion  // Del presupuesto
                     ?? cliente.PorcentajeRetencionDefecto, // O del cliente
                Bloqueada = false,
                ActualizadoEn = DateTime.UtcNow
            };

            // Cargar productos para configuración fiscal
            var lineasACopiar = presupuesto.Lineas.AsEnumerable();

            if (dto.LineasSeleccionadas != null && dto.LineasSeleccionadas.Any())
            {
                lineasACopiar = lineasACopiar.Where(l => dto.LineasSeleccionadas.Contains(l.Id));
            }

            var productosIds = lineasACopiar
                .Where(l => l.ProductoId.HasValue)
                .Select(l => l.ProductoId.Value)
                .Distinct()
                .ToList();

            Dictionary<int, Producto> productosDict = new();

            if (productosIds.Any())
            {
                var productos = await _context.Productos
                    .Where(p => productosIds.Contains(p.Id))
                    .ToListAsync();

                productosDict = productos.ToDictionary(p => p.Id);
            }

            int orden = 1;
            foreach (var lineaPresupuesto in lineasACopiar.OrderBy(l => l.Orden))
            {
                // Aplicar modificaciones si existen
                var cantidad = lineaPresupuesto.Cantidad;
                var precioUnitario = lineaPresupuesto.PrecioUnitario;

                if (dto.LineasModificadas != null)
                {
                    var modificacion = dto.LineasModificadas
                        .FirstOrDefault(m => m.LineaId == lineaPresupuesto.Id);

                    if (modificacion != null)
                    {
                        cantidad = modificacion.Cantidad ?? cantidad;
                        precioUnitario = modificacion.PrecioUnitario ?? precioUnitario;
                    }
                }

                // ⭐ MANTENER CONFIGURACIÓN FISCAL DEL PRESUPUESTO
                // (El presupuesto ya tenía la configuración correcta)
                decimal iva = lineaPresupuesto.IVA;
                decimal recargo = lineaPresupuesto.RecargoEquivalencia;

                // ⭐ PERO SI EL CLIENTE CAMBIÓ SU RÉGIMEN, RECALCULAR
                // (Esto puede pasar si el cliente actualizó su configuración entre presupuesto y factura)
                if (lineaPresupuesto.ProductoId.HasValue &&
                    productosDict.ContainsKey(lineaPresupuesto.ProductoId.Value))
                {
                    var producto = productosDict[lineaPresupuesto.ProductoId.Value];

                    // Solo recalcular recargo si el cliente AHORA está en régimen y antes NO lo estaba
                    if (cliente.RegimenRecargoEquivalencia && lineaPresupuesto.RecargoEquivalencia == 0)
                    {
                        recargo = producto.RecargoEquivalenciaDefecto ?? 0m;

                        _logger.LogInformation(
                            "Cliente {Cliente} ahora en régimen recargo. Aplicando {Recargo}% a línea",
                            cliente.Nombre, recargo);
                    }
                }

                var lineaFactura = new LineaFactura
                {
                    Orden = orden++,
                    Descripcion = lineaPresupuesto.Descripcion,
                    Cantidad = cantidad,
                    PrecioUnitario = precioUnitario,
                    IVA = iva,
                    RecargoEquivalencia = recargo, // ⭐ MANTENIDO DEL PRESUPUESTO
                    ProductoId = lineaPresupuesto.ProductoId,
                    Importe = Math.Round(cantidad * precioUnitario, 2)
                };

                factura.Lineas.Add(lineaFactura);
            }

            // Calcular totales
            CalcularTotalesFactura(factura);

            _logger.LogInformation(
                "Factura desde presupuesto {Presupuesto}. Recargo: {Recargo}€, Retención: {Retencion}€",
                presupuesto.Numero, factura.TotalRecargo, factura.CuotaRetencion);

            // Obtener hash de la factura anterior
            var facturaAnterior = await _context.Facturas
                .Where(f => f.SerieId == dto.SerieId && f.TenantId == tenantId)
                .OrderByDescending(f => f.FechaEmision)
                .ThenByDescending(f => f.Id)
                .FirstOrDefaultAsync();

            factura.HuellaAnterior = facturaAnterior?.Huella;

            // Aplicar VERIFACTU
            await AplicarVERIFACTUAsync(factura);

            _context.Facturas.Add(factura);
            await _context.SaveChangesAsync();

            // Vincular presupuesto con factura
            presupuesto.FacturaId = factura.Id;
            presupuesto.Estado = "Facturado";
            await _context.SaveChangesAsync();

            _logger.LogInformation("Creada factura {NumeroFactura} desde presupuesto {NumeroPresupuesto}",
                numeroCompleto, presupuesto.Numero);

            // Enviar a AEAT en background
            _ = Task.Run(async () =>
            {
                try
                {
                    await EnviarFacturaAEATAsync(factura.Id, tenantId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar factura {FacturaId} a AEAT", factura.Id);
                }
            });

            return await MapearAResponseDto(factura);
        }


        // ====================================================================
        // CONVERTIR DESDE ALBARANES
        // ====================================================================
        // API/Services/FacturaService.cs - ConvertirDesdeAlbaranAsync

        public async Task<FacturaResponseDto> ConvertirDesdeAlbaranAsync(
            int tenantId, int id, ConvertirAlbaranesAFacturaDto dto)
        {
            // Validar albaranes CON CLIENTE
            var albaranes = await _context.Albaranes
                .Include(a => a.Lineas)
                .Include(a => a.Cliente) // ⭐ IMPORTANTE
                .Where(a => dto.AlbaranesIds.Contains(a.Id) && a.TenantId == tenantId)
                .ToListAsync();

            if (albaranes.Count != dto.AlbaranesIds.Count)
                throw new InvalidOperationException("Algunos albaranes no fueron encontrados");

            // Validar que todos sean del mismo cliente
            var clienteId = albaranes.First().ClienteId;
            if (albaranes.Any(a => a.ClienteId != clienteId))
                throw new InvalidOperationException("Todos los albaranes deben ser del mismo cliente");

            if (albaranes.Any(a => a.Facturado || a.FacturaId != null))
                throw new InvalidOperationException("Algunos albaranes ya están facturados");

            // ⭐ OBTENER CLIENTE CON CONFIGURACIÓN FISCAL
            var cliente = albaranes.First().Cliente;

            // Validar serie
            var serie = await _context.SeriesNumeracion
                .FirstOrDefaultAsync(s => s.Id == dto.SerieId && s.TenantId == tenantId);

            if (serie == null)
                throw new InvalidOperationException($"Serie {dto.SerieId} no encontrada");

            if (serie.Bloqueada)
                throw new InvalidOperationException("La serie está bloqueada");

            // Obtener siguiente número
            var ejercicio = dto.FechaEmision?.Year ?? DateTime.UtcNow.Year;
            var (numeroCompleto, numero) = await _numeracionService
                .ObtenerSiguienteNumeroAsync(tenantId, serie.Codigo, ejercicio, "FACTURA");

            // Crear Factura
            var factura = new Factura
            {
                TenantId = tenantId,
                ClienteId = clienteId,
                SerieId = serie.Id,
                Numero = numeroCompleto,
                Ejercicio = ejercicio,
                FechaEmision = (dto.FechaEmision ?? DateTime.UtcNow).ToUniversalTime(),
                Estado = "Emitida",
                Observaciones = dto.Observaciones ??
                    $"Factura generada desde albaranes: {string.Join(", ", albaranes.Select(a => a.Numero))}",
                TipoFacturaVERIFACTU = dto.TipoFacturaVERIFACTU,

                // ⭐ APLICAR RETENCIÓN DEL CLIENTE
                PorcentajeRetencion = dto.PorcentajeRetencion
                    ?? cliente.PorcentajeRetencionDefecto,

                Bloqueada = false,
                ActualizadoEn = DateTime.UtcNow
            };

            // Cargar productos para posibles recálculos
            var productosIds = albaranes
                .SelectMany(a => a.Lineas)
                .Where(l => l.ProductoId.HasValue)
                .Select(l => l.ProductoId.Value)
                .Distinct()
                .ToList();

            Dictionary<int, Producto> productosDict = new();

            if (productosIds.Any())
            {
                var productos = await _context.Productos
                    .Where(p => productosIds.Contains(p.Id))
                    .ToListAsync();

                productosDict = productos.ToDictionary(p => p.Id);
            }

            // Copiar líneas de todos los albaranes
            int orden = 1;
            foreach (var albaran in albaranes.OrderBy(a => a.FechaEmision))
            {
                foreach (var lineaAlbaran in albaran.Lineas.OrderBy(l => l.Orden))
                {
                    // ⭐ MANTENER CONFIGURACIÓN FISCAL DEL ALBARÁN
                    decimal iva = lineaAlbaran.IVA;
                    decimal recargo = lineaAlbaran.RecargoEquivalencia;

                    // ⭐ VERIFICAR SI EL CLIENTE CAMBIÓ SU RÉGIMEN
                    if (lineaAlbaran.ProductoId.HasValue &&
                        productosDict.ContainsKey(lineaAlbaran.ProductoId.Value))
                    {
                        var producto = productosDict[lineaAlbaran.ProductoId.Value];

                        // Si cliente ahora en recargo y albaran no lo tenía
                        if (cliente.RegimenRecargoEquivalencia && lineaAlbaran.RecargoEquivalencia == 0)
                        {
                            recargo = producto.RecargoEquivalenciaDefecto ?? 0m;

                            _logger.LogInformation(
                                "Cliente {Cliente} ahora en régimen recargo. Aplicando {Recargo}%",
                                cliente.Nombre, recargo);
                        }
                    }

                    var lineaFactura = new LineaFactura
                    {
                        Orden = orden++,
                        Descripcion = $"[{albaran.Numero}] {lineaAlbaran.Descripcion}",
                        Cantidad = lineaAlbaran.Cantidad,
                        PrecioUnitario = lineaAlbaran.PrecioUnitario,
                        IVA = iva,
                        RecargoEquivalencia = recargo, // ⭐ MANTENIDO DEL ALBARÁN
                        ProductoId = lineaAlbaran.ProductoId,
                        Importe = Math.Round(lineaAlbaran.Cantidad * lineaAlbaran.PrecioUnitario, 2)
                    };

                    factura.Lineas.Add(lineaFactura);
                }
            }

            // Calcular totales
            CalcularTotalesFactura(factura);

            _logger.LogInformation(
                "Factura desde albaranes. Recargo: {Recargo}€, Retención: {Retencion}€",
                factura.TotalRecargo, factura.CuotaRetencion);

            // Obtener hash de la factura anterior
            var facturaAnterior = await _context.Facturas
                .Where(f => f.SerieId == dto.SerieId && f.TenantId == tenantId)
                .OrderByDescending(f => f.FechaEmision)
                .ThenByDescending(f => f.Id)
                .FirstOrDefaultAsync();

            factura.HuellaAnterior = facturaAnterior?.Huella;

            // Aplicar VERIFACTU
            await AplicarVERIFACTUAsync(factura);

            _context.Facturas.Add(factura);
            await _context.SaveChangesAsync();

            // Vincular Albaranes con factura
            foreach (var albaran in albaranes)
            {
                albaran.FacturaId = factura.Id;
                albaran.Facturado = true;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Creada factura {NumeroFactura} desde albaranes [{Albaranes}]",
                numeroCompleto, string.Join(", ", albaranes.Select(a => a.Numero)));

            // Enviar a AEAT en background
            _ = Task.Run(async () =>
            {
                try
                {
                    await EnviarFacturaAEATAsync(factura.Id, tenantId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar factura {FacturaId} a AEAT", factura.Id);
                }
            });

            return await MapearAResponseDto(factura);
        }

        #region Métodos privados

        // ====================================================================
        // ⭐ CALCULAR TOTALES FACTURA - NORMATIVA ESPAÑOLA
        // ====================================================================
        private void CalcularTotalesFactura(Factura factura)
        {
            // Base Imponible = Σ(Cantidad × Precio) de todas las líneas
            factura.BaseImponible = factura.Lineas.Sum(l => l.Importe);

            // Total IVA = Σ(Base línea × IVA% línea)
            factura.TotalIVA = Math.Round(
                factura.Lineas.Sum(l => l.Importe * l.IVA / 100), 2);

            // Total Recargo = Σ(Base línea × Recargo% línea)
            factura.TotalRecargo = Math.Round(
                factura.Lineas.Sum(l => l.Importe * l.RecargoEquivalencia / 100), 2);

            // Cuota Retención = Base Imponible Total × Retención%
            factura.CuotaRetencion = Math.Round(
                factura.BaseImponible * (factura.PorcentajeRetencion ??0m) / 100, 2);

            // TOTAL FACTURA = Base + IVA + Recargo - Retención
            factura.Total = factura.BaseImponible
                          + factura.TotalIVA
                          + factura.TotalRecargo
                          - factura.CuotaRetencion ??0m;
        }


        // ====================================================================
        // APLICAR VERIFACTU
        // ====================================================================
        public async Task AplicarVERIFACTUAsync(Factura factura)
        {
            if (factura.Tenant == null)
            {
                factura.Tenant = await _context.Tenants.FindAsync(factura.TenantId);
            }

            if (factura.Cliente == null)
            {
                factura.Cliente = await _context.Clientes.FindAsync(factura.ClienteId);
            }

            if (factura.Serie == null)
            {
                factura.Serie = await _context.SeriesNumeracion.FindAsync(factura.SerieId);
            }

            _verifactuService.ProcesarFacturaVERIFACTU(factura, factura.HuellaAnterior ?? "");
        }


        // ====================================================================
        // ENVIAR FACTURA A AEAT (Background)
        // ====================================================================
        private async Task EnviarFacturaAEATAsync(int facturaId, int tenantId)
        {
            try
            {
                // Crear un nuevo Scope para evitar problemas con DBContext
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var verifactuService = scope.ServiceProvider.GetRequiredService<VERIFACTUService>();

                var factura = await context.Facturas
                    .Include(f => f.Tenant)
                    .Include(f => f.Cliente)
                    .Include(f => f.Serie)
                    .Include(f => f.Lineas)
                    .FirstOrDefaultAsync(f => f.Id == facturaId);

                if (factura == null)
                {
                    _logger.LogWarning("Factura {FacturaId} no encontrada para envío a AEAT", facturaId);
                    return;
                }

                // Enviar a AEAT
                var resultado = await verifactuService.ProcesarYEnviarFactura(facturaId);

                // Actualizar estado
                factura.EnviadaVERIFACTU = resultado;
                factura.FechaEnvioVERIFACTU = DateTime.UtcNow;

                await context.SaveChangesAsync();

                _logger.LogInformation("Factura {FacturaId} enviada a AEAT: {Estado}",
                    facturaId, resultado ? "OK" : "ERROR");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al enviar factura {FacturaId} a AEAT", facturaId);
            }
        }


        // ====================================================================
        // MAPEAR A RESPONSE DTO
        // ====================================================================
        private async Task<FacturaResponseDto> MapearAResponseDto(Factura factura)
        {
            // Cargar relaciones si no están cargadas
            if (factura.Cliente == null)
            {
                factura.Cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.Id == factura.ClienteId);
            }

            var serie = await _context.SeriesNumeracion
                .FirstOrDefaultAsync(s => s.Id == factura.SerieId);

            // Cargar albaranes si existen
            var albaranesIds = await _context.Albaranes
                .Where(a => a.FacturaId == factura.Id)
                .Select(a => a.Id)
                .ToListAsync();

            return new FacturaResponseDto
            {
                Id = factura.Id,
                TenantId = factura.TenantId,
                ClienteId = factura.ClienteId,
                ClienteNombre = factura.Cliente?.Nombre,
                ClienteNIF = factura.Cliente?.NIF,
                SerieId = factura.SerieId,
                SerieCodigo = serie?.Codigo,
                Numero = factura.Numero,
                Ejercicio = factura.Ejercicio,
                FechaEmision = factura.FechaEmision,

                // ⭐ TOTALES ACTUALIZADOS
                BaseImponible = factura.BaseImponible,
                TotalIva = factura.TotalIVA,
                TotalRecargo = factura.TotalRecargo ?? 0m, // ⭐ NUEVO
                PorcentajeRetencion = factura.PorcentajeRetencion ?? 0m, // ⭐ NUEVO
                CuotaRetencion = factura.CuotaRetencion ?? 0m, // ⭐ NUEVO
                Total = factura.Total,

                Estado = factura.Estado,
                Bloqueada = factura.Bloqueada, // ⭐ NUEVO
                Observaciones = factura.Observaciones,

                Huella = factura.Huella,
                HuellaAnterior = factura.HuellaAnterior,
                EnviadaVERIFACTU = factura.EnviadaVERIFACTU,
                FechaEnvioVERIFACTU = factura.FechaEnvioVERIFACTU,
                TipoFacturaVERIFACTU = factura.TipoFacturaVERIFACTU,
                UrlVERIFACTU = factura.UrlVERIFACTU,
                QRBase64 = factura.QRVerifactu != null
                    ? Convert.ToBase64String(factura.QRVerifactu)
                    : null,
                NumeroFacturaRectificada = factura.NumeroFacturaRectificada,
                TipoRectificacion = factura.TipoRectificacion,
                AlbaranesIds = albaranesIds,

                Lineas = factura.Lineas.Select(l => new LineaFacturaResponseDto
                {
                    Id = l.Id,
                    Orden = l.Orden,
                    ProductoId = l.ProductoId,
                    ProductoCodigo = l.Producto?.Codigo,
                    Descripcion = l.Descripcion,
                    Cantidad = l.Cantidad,
                    PrecioUnitario = l.PrecioUnitario,
                    PorcentajeDescuento = 0,
                    ImporteDescuento = 0,
                    BaseImponible = l.Importe,
                    IVA = l.IVA,
                    RecargoEquivalencia = l.RecargoEquivalencia, // ⭐ NUEVO
                    ImporteIva = Math.Round(l.Importe * l.IVA / 100, 2),
                    ImporteRecargo = Math.Round(l.Importe * l.RecargoEquivalencia / 100, 2), // ⭐ NUEVO
                    Importe = l.Importe
                }).OrderBy(l => l.Orden).ToList(),

                FechaCreaccion = DateTime.UtcNow,
                FechaModificacion = factura.ActualizadoEn // ⭐ NUEVO
            };
        }

        #endregion
    }
}