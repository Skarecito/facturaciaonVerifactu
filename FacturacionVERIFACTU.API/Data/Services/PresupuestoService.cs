using FacturacionVERIFACTU.API.Data;
using FacturacionVERIFACTU.API.Data.Entities;
using FacturacionVERIFACTU.API.Data.Interfaces;
using FacturacionVERIFACTU.API.DTOs;
using FacturacionVERIFACTU.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;


namespace FacturacionVERIFACTU.API.Data.Services
{
    public interface IPresupuestoService
    {
        Task<PresupuestoResponseDto> CrearPresupuestoAsync(int tenantId, PresupuestoCreateDto dto);
        Task<PresupuestoResponseDto> ActualizarPresupuestoAsync(int tenantId, int id, PresupuestoUpdateDto dto);
        Task<PresupuestoResponseDto> CambiarEstadoAsync(int tenantId, int id , CambiarEstadoPresupuestoDto dto);
        Task<PresupuestoResponseDto> ObtenerPorIdAsync(int tenantId, int id);
        Task<List<PresupuestoResponseDto>> ObtenerTodosAsync(int tenantId, string? estado =null);
        Task<bool> EliminarAsync(int tenantId, int id);
    }

    public class PresupuestoService : IPresupuestoService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISerieNumeracionService _numeracionService;
        private readonly ILogger<PresupuestoService> _logger;

        public PresupuestoService(
            ApplicationDbContext context,
            ISerieNumeracionService numeracionService,
            ILogger<PresupuestoService> logger )
        {
            _context = context;
            _numeracionService = numeracionService;
            _logger = logger;
        }

        public async Task<PresupuestoResponseDto> CrearPresupuestoAsync(int tenantId, PresupuestoCreateDto dto)
        {
            // Cargar cliente con configuración fiscal
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == dto.ClienteId && c.TenantId == tenantId);

            if (cliente == null)
                throw new InvalidOperationException("Cliente no encontrado");

            // ... validaciones de productos (igual que factura) ...

            // Validar serie
            var serie = await _context.SeriesNumeracion
                .FirstOrDefaultAsync(s => s.Id == dto.SerieId && s.TenantId == tenantId);

            if (serie == null)
                throw new InvalidOperationException($"Serie {dto.SerieId} no encontrada");

            // Obtener siguiente número
            var ejercicio = dto.Fecha?.Year ?? DateTime.UtcNow.Year;
            var (numeroCompleto, numero) = await _numeracionService
                .ObtenerSiguienteNumeroAsync(tenantId, serie.Codigo, ejercicio, DocumentTypes.PRESUPUESTO);

            // ⭐ APLICAR RETENCIÓN DEL CLIENTE
            var porcentajeRetencion = dto.PorcentajeRetencion
                ?? cliente.PorcentajeRetencionDefecto;

            var presupuesto = new Presupuesto
            {
                TenantId = tenantId,
                ClienteId = dto.ClienteId,
                SerieId = dto.SerieId,
                Numero = numeroCompleto,
                Ejercicio = ejercicio,
                Fecha = dto.Fecha ?? DateTime.UtcNow,
                FechaValidez = dto.FechaValidez ?? DateTime.UtcNow.AddDays(15),
                Estado = "Borrador",
                Observaciones = dto.Observaciones,
                PorcentajeRetencion = porcentajeRetencion, // ⭐ DESDE CLIENTE
                FechaCreacion = DateTime.UtcNow
            };

            // ⭐ AGREGAR LÍNEAS CON CONFIGURACIÓN AUTOMÁTICA
            int orden = 1;
            foreach (var lineaDto in dto.Lineas)
            {
                decimal iva = lineaDto.IVA ?? 21m;
                decimal? recargo = 0;

                if (lineaDto.ArticuloId.HasValue)
                {
                    var producto = await _context.Productos.FindAsync(lineaDto.ArticuloId.Value);
                    if (producto != null)
                    {
                        iva = lineaDto.IVA ?? producto.IVADefecto;

                        if (cliente.RegimenRecargoEquivalencia)
                        {
                            recargo = lineaDto.RecargoEquivalencia
                                ?? producto.RecargoEquivalenciaDefecto;
                        }
                    }
                }
                else if (cliente.RegimenRecargoEquivalencia)
                {
                    recargo = lineaDto.RecargoEquivalencia
                        ?? FiscalConfiguracionService.CalcularRecargoEquivalencia(iva);
                }

                var linea = new LineaPresupuesto
                {
                    Orden = orden++,
                    Descripcion = lineaDto.Descripcion,
                    Cantidad = lineaDto.Cantidad,
                    PrecioUnitario = lineaDto.PrecioUnitario,
                    PorcentajeDescuento = lineaDto.PorcentajeDescuento,
                    IVA = iva,
                    RecargoEquivalencia = recargo ?? 0m, // ⭐ APLICADO
                    ProductoId = lineaDto.ArticuloId
                };

                CalcularLinea(linea);

                presupuesto.Lineas.Add(linea);
            }

            CalcularTotalesPrespuesto(presupuesto);


            _context.Presupuestos.Add(presupuesto);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Creado presupuesto {NumeroPresupuesto} para tenant {TenantId}",
                numeroCompleto, tenantId);

            return await MapearAResponseDto(presupuesto);
        }


        // API/Services/PresupuestoService.cs - ActualizarPresupuestoAsync

        public async Task<PresupuestoResponseDto> ActualizarPresupuestoAsync(
            int tenantId, int id, PresupuestoUpdateDto dto)
        {
            var presupuesto = await _context.Presupuestos
                .Include(p => p.Lineas)
                .Include(p => p.Cliente) // ⭐ INCLUIR CLIENTE
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

            if (presupuesto == null)
                throw new InvalidOperationException("Presupuesto no encontrado");

            // Validaciones de estado
            if (presupuesto.Estado == "Aceptado")
                throw new InvalidOperationException("No se puede modificar un presupuesto aceptado");

            if (presupuesto.Estado == "Rechazado")
                throw new InvalidOperationException("No se puede modificar un presupuesto rechazado");

            if (presupuesto.Estado == "Facturado")
                throw new InvalidOperationException("No se puede modificar un presupuesto facturado");

            // ⭐ SI CAMBIÓ EL CLIENTE, RECARGAR CONFIGURACIÓN FISCAL
            Cliente clienteActual = presupuesto.Cliente;

            if (dto.ClienteId != presupuesto.ClienteId)
            {
                clienteActual = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.Id == dto.ClienteId && c.TenantId == tenantId);

                if (clienteActual == null)
                    throw new InvalidOperationException("Cliente no encontrado");

                presupuesto.ClienteId = dto.ClienteId;

                // ⭐ RECALCULAR RETENCIÓN
                presupuesto.PorcentajeRetencion = dto.PorcentajeRetencion
                    ?? clienteActual.PorcentajeRetencionDefecto;
            }
            else if (dto.PorcentajeRetencion.HasValue)
            {
                presupuesto.PorcentajeRetencion = dto.PorcentajeRetencion.Value;
            }

            // Validar productos
            var lineasProductoIds = dto.Lineas
                .Where(l => l.ArticuloId.HasValue)
                .Select(l => l.ArticuloId.Value)
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
                    throw new InvalidOperationException($"Producto {idFaltante} no encontrado");
                }

                productosDict = productos.ToDictionary(p => p.Id);
            }

            // Actualizar datos básicos
            presupuesto.FechaCreacion = dto.FechaEmision ?? presupuesto.Fecha;
            presupuesto.FechaValidez = dto.FechaValidez ?? presupuesto.FechaValidez;
            presupuesto.Observaciones = dto.Observaciones;
            presupuesto.FechaModificacion = DateTime.UtcNow;

            // Eliminar líneas antiguas
            var lineasDtoIds = dto.Lineas
                .Where(l => l.Id.HasValue)
                .Select(l => l.Id.Value)
                .ToList();

            var lineasAEliminar = presupuesto.Lineas
                .Where(l => !lineasDtoIds.Contains(l.Id))
                .ToList();

            foreach (var linea in lineasAEliminar)
            {
                _context.LineasPresupuesto.Remove(linea);
            }

            // Actualizar/Agregar líneas
            int orden = 1;
            foreach (var lineaDto in dto.Lineas)
            {
                decimal iva = lineaDto.IVA ?? 21m;
                decimal? recargo = 0;

                // ⭐ APLICAR CONFIGURACIÓN FISCAL DEL PRODUCTO
                if (lineaDto.ArticuloId.HasValue && productosDict.ContainsKey(lineaDto.ArticuloId.Value))
                {
                    var producto = productosDict[lineaDto.ArticuloId.Value];

                    if (!lineaDto.IVA.HasValue || lineaDto.IVA == 0)
                    {
                        iva = producto.IVADefecto;
                    }

                    if (clienteActual.RegimenRecargoEquivalencia)
                    {
                        recargo = lineaDto.RecargoEquivalencia
                            ?? producto.RecargoEquivalenciaDefecto;
                    }
                }
                else if (clienteActual.RegimenRecargoEquivalencia)
                {
                    recargo = lineaDto.RecargoEquivalencia
                        ?? FiscalConfiguracionService.CalcularRecargoEquivalencia(iva);
                }

                if (lineaDto.Id.HasValue && lineaDto.Id.Value > 0)
                {
                    // Actualizar existente
                    var lineaExistente = presupuesto.Lineas
                        .FirstOrDefault(l => l.Id == lineaDto.Id.Value);

                    if (lineaExistente != null)
                    {
                        lineaExistente.Orden = orden++;
                        lineaExistente.Descripcion = lineaDto.Descripcion;
                        lineaExistente.Cantidad = lineaDto.Cantidad;
                        lineaExistente.PrecioUnitario = lineaDto.PrecioUnitario;
                        lineaExistente.PorcentajeDescuento = lineaDto.PorcentajeDescuento;
                        lineaExistente.IVA = iva;                          // ⭐ RECALCULADO
                        lineaExistente.RecargoEquivalencia = recargo ?? 0m;       // ⭐ RECALCULADO
                        lineaExistente.ProductoId = lineaDto.ArticuloId;

                        CalcularLinea(lineaExistente);
                    }
                }
                else
                {
                    // Crear nueva
                    var lineaNueva = new LineaPresupuesto
                    {
                        PresupuestoId = presupuesto.Id,
                        Orden = orden++,
                        Descripcion = lineaDto.Descripcion,
                        Cantidad = lineaDto.Cantidad,
                        PrecioUnitario = lineaDto.PrecioUnitario,
                        PorcentajeDescuento = lineaDto.PorcentajeDescuento,
                        IVA = iva,                                  // ⭐ CALCULADO
                        RecargoEquivalencia = recargo ??0m,              // ⭐ CALCULADO
                        ProductoId = lineaDto.ArticuloId
                    };

                    CalcularLinea(lineaNueva);

                    presupuesto.Lineas.Add(lineaNueva);
                }
            }

            // Recalcular totales
            CalcularTotalesPrespuesto(presupuesto);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Actualizado presupuesto {Numero} para tenant {TenantId}",
                presupuesto.Numero, tenantId);

            return await MapearAResponseDto(presupuesto);
        }

        public async Task<PresupuestoResponseDto> CambiarEstadoAsync(
            int tenantId,
            int id,
            CambiarEstadoPresupuestoDto dto)
        {
            var presupuesto = await _context.Presupuestos
                .Include(p => p.Lineas)
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

            if (presupuesto == null)
            {
                throw new InvalidOperationException($"Presupuesto {id} no encontrado");
            }

            // Validar transición de estado
            ValidarTransicionEstado(presupuesto.Estado, dto.NuevoEstado);

            var estadoAnterior = presupuesto.Estado;
            presupuesto.Estado = dto.NuevoEstado;
            presupuesto.FechaModificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Cambiado estado de presupuesto {NumeroPresupuesto} de {EstadoAnterior} a {EstadoNuevo}",
                presupuesto.Numero, estadoAnterior, dto.NuevoEstado);

            return await MapearAResponseDto(presupuesto);
        }

        public async Task<PresupuestoResponseDto?> ObtenerPorIdAsync(int tenantId, int id)
        {
            var prespuesto = await _context.Presupuestos
                .Include(p=>p.Lineas)
                .Include(p=>p.Cliente)
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

            if (prespuesto == null) return null;

            return await MapearAResponseDto(prespuesto);
        }

        public async Task<List<PresupuestoResponseDto>> ObtenerTodosAsync(
            int tenantId,
            string? estado = null)
        {
            var query = _context.Presupuestos
                .Include(p => p.Lineas)
                .Include(p => p.Cliente)
                .Where(p => p.TenantId == tenantId);

            if (!string.IsNullOrEmpty(estado))
            {
                query = query.Where(p=> p.Estado == estado);
            }

            var prespuestos = await query
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();

            var result = new List<PresupuestoResponseDto>();
            foreach (var presupuesto in prespuestos)
            {
                result.Add(await MapearAResponseDto(presupuesto));
            }

            return result;
        }

        public async Task<bool> EliminarAsync(int tenantId, int id)
        {
            var presupuesto = await _context.Presupuestos
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

            if (presupuesto == null) return false;

            //Solo se pueden eliminar prespuestos en estado Borrador
            if(presupuesto.Estado != "Borrador")
            {
                throw new InvalidOperationException(
                    $"No se puede eliminar un presupuesto en estado {presupuesto.Estado}");
            }

            _context.Presupuestos.Remove(presupuesto);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
               "Eliminado presupuesto {NumeroPresupuesto} del tenant {TenantId}",
               presupuesto.Numero, tenantId);

            return true;
        }

        #region Metodos privados

        ///<summary>
        ///Calcula los impoertes de una linea
        /// </summary>
        private void CalcularLinea(LineaPresupuesto linea)
        {
            //Subtotal = Cantidad + precio
            var subTotal = Math.Round(linea.Cantidad * linea.PrecioUnitario, 2);

            //Descuento
            linea.ImporteDescuento = Math.Round(subTotal * (linea.PorcentajeDescuento / 100), 2);

            //Base imponible = Subtotal - Descuento
            linea.BaseImponible = Math.Round(subTotal - linea.ImporteDescuento, 2);

            //Iva
            linea.ImporteIva = Math.Round(linea.BaseImponible * (linea.IVA / 100), 2);

            //Recargo
            linea.ImporteRecargo = Math.Round(linea.BaseImponible * (linea.RecargoEquivalencia / 100), 2);

            //Total linea
            linea.Importe = Math.Round(linea.BaseImponible + linea.ImporteIva + linea.ImporteRecargo, 2);
        }

        ///<summary>
        ///Calcula los totales del presupuesto
        /// </summary>
        private void CalcularTotalesPrespuesto(Presupuesto presupuesto)
        {
            presupuesto.BaseImponible = Math.Round(presupuesto.Lineas.Sum(l => l.BaseImponible), 2);
            presupuesto.TotalIva = Math.Round(presupuesto.Lineas.Sum(l => l.ImporteIva), 2);
            presupuesto.TotalRecargo = Math.Round(presupuesto.Lineas.Sum(l => l.ImporteRecargo), 2);

            var porcentajeRetencion = presupuesto.PorcentajeRetencion ?? 0m;
            presupuesto.CuotaRetencion = Math.Round(presupuesto.BaseImponible * porcentajeRetencion / 100, 2);

            var totalRecargo = presupuesto.TotalRecargo ?? 0m;
            var cuotaRetencion = presupuesto.CuotaRetencion ?? 0m;
            presupuesto.Total = Math.Round(
                presupuesto.BaseImponible + presupuesto.TotalIva + totalRecargo - cuotaRetencion, 2);
        }

        /// <summary>
        /// Valida que la transición de estado sea permitida
        /// </summary>
        private void ValidarTransicionEstado(string estadoActual, string nuevoEstado)
        {
            var transicionesPermitidas = new Dictionary<string, List<string>>
            {
                { "Borrador", new List<string> { "Enviado" } },
                { "Enviado", new List<string> { "Aceptado", "Rechazado", "Borrador" } },
                { "Aceptado", new List<string>() }, // Estado final
                { "Rechazado", new List<string> { "Borrador" } },  // Permite volver a Borrador
                {"Facturado", new List<string>() }
            };

            if (!transicionesPermitidas.ContainsKey(estadoActual))
            {
                throw new InvalidOperationException($"Estado actual '{estadoActual}' no válido");
            }

            if (!transicionesPermitidas[estadoActual].Contains(nuevoEstado))
            {
                throw new InvalidOperationException(
                    $"No se puede cambiar de '{estadoActual}' a '{nuevoEstado}'");
            }
        }

        /// <summary>
        /// Mapea entidad a DTO de respuesta
        /// </summary>
        private async Task<PresupuestoResponseDto> MapearAResponseDto(Presupuesto presupuesto)
        {
            // Cargar cliente si no está cargado
            if (presupuesto.Cliente == null)
            {
                presupuesto.Cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.Id == presupuesto.ClienteId);
            }

            return new PresupuestoResponseDto
            {
                Id = presupuesto.Id,
                TenantId = presupuesto.TenantId,
                NumeroPresupuesto = presupuesto.Numero,
                SerieId = presupuesto.SerieId,
                Ejercicio = presupuesto.Ejercicio,
                FechaEmision = presupuesto.Fecha,
                FechaValidez = presupuesto.FechaValidez,
                ClienteId = presupuesto.ClienteId,
                ClienteNombre = presupuesto.Cliente?.Nombre,
                Estado = presupuesto.Estado,
                BaseImponible = presupuesto.BaseImponible,
                TotalIVA = presupuesto.TotalIva,
                TotalRecargo = presupuesto.TotalRecargo ?? 0m,
                PorcentajeRetencion = presupuesto.PorcentajeRetencion ?? 0m,
                CuotaRetencion = presupuesto.CuotaRetencion ?? 0m,
                TotalConRetencion = presupuesto.TotalConRetencion ?? 0m,
                Total = presupuesto.Total,
                Observaciones = presupuesto.Observaciones,
                Lineas = presupuesto.Lineas.Select(l => new LineaPresupuestoResponseDto
                {
                    Id = l.Id,
                    Orden = l.Orden,
                    Descripcion = l.Descripcion,
                    Cantidad = l.Cantidad,
                    PrecioUnitario = l.PrecioUnitario,
                    PorcentajeDescuento = l.PorcentajeDescuento,
                    ImporteDescuento = l.ImporteDescuento,
                    BaseImponible = l.BaseImponible,
                    IVA = l.IVA,
                    ImporteIVA = l.ImporteIva,
                    ImporteRecargo = l.ImporteRecargo,
                    RecargoEquivalencia = l.RecargoEquivalencia,
                    Total = l.Importe,
                    ArticuloId = l.ProductoId,
                    ArticuloCodigo = l.Producto?.Codigo
                }).ToList(),
                FechaCreacion = presupuesto.FechaCreacion,
                FechaModificacion = presupuesto.FechaModificacion
            };
        }

        #endregion
    }
}

