using API.Data.Entities;
using FacturacionVERIFACTU.API.DTOs;
using Microsoft.EntityFrameworkCore;
using FacturacionVERIFACTU.API.Data.Services;


namespace FacturacionVERIFACTU.API.Data.Services
{
    public interface IAlbaranService
    {
        Task<AlbaranResponseDto> CrearAlbaranAsync(int tenantId, AlbaranCreateDto dto);
        Task<AlbaranResponseDto> ActualizarAlbaranAsync(int tenantId, int id, AlbaranUpdateDto dto);
        Task<AlbaranResponseDto>CambiarEstadoAsync(int tenantId, int id, CambiarEstadoAlbaranDto dto);
        Task<AlbaranResponseDto> ObtenerPorIdAsync(int tenantId, int id);
        Task<List<AlbaranResponseDto>> ObtenerTodosAsync(int tenantId, string? estado = null);
        Task<bool>EliminarAsync (int tenantId, int id);
        Task<AlbaranResponseDto> ConvertirDesdePresupuesto(int tenantId, int presupuestoId, ConvertirPresupuestoDto dto);
    }

    public class AlbaranService : IAlbaranService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISerieNumeracionService _numeracionService;
        private readonly ILogger<AlbaranService> _logger;

        public AlbaranService(ApplicationDbContext context, ISerieNumeracionService numeracionService, ILogger<AlbaranService> logger)
        {
            _context = context;
            _numeracionService = numeracionService;
            _logger = logger;
        }

        public async Task<AlbaranResponseDto> CrearAlbaranAsync(int tenantId, AlbaranCreateDto dto)
        {
            //Validar si el cliente exite
            var cliente = await _context.Clientes
        .FirstOrDefaultAsync(c => c.Id == dto.ClienteId && c.TenantId == tenantId);

            if (cliente == null)
                throw new InvalidOperationException($"Cliente {dto.ClienteId} no encontrado");

            //Validar serie
            var serie = await _context.SeriesNumeracion
                .FirstOrDefaultAsync(s=> s.Id == dto.SerieId && s.TenantId == tenantId);
            if (serie == null)
                throw new InvalidOperationException($"Serie {dto.SerieId} no encontrada");

            //Obtener el siguiente numero
            var ejercicio = dto.FechaEmision?.Year ?? DateTime.UtcNow.Year;
            var (numeroCompleto, numero) = await _numeracionService
                .ObtenerSiguienteNumeroAsync(tenantId, serie.Codigo, ejercicio, "ALBARAN");

            //Crear albaran
            var albaran = new Albaran
            {
                TenantId = tenantId,
                ClienteId = dto.ClienteId,
                SerieId = dto.SerieId,
                PresupuestoId = dto.PresupuestoId,
                Numero = numeroCompleto,
                Ejercicio = ejercicio,
                FechaEmision = (dto.FechaEmision ?? DateTime.UtcNow).ToUniversalTime(),
                FechaEntrega = dto.FechaEntrega?.ToUniversalTime(),
                Estado = "Pendiente",
                DireccionEntrega = dto.DireccionEntrega,
                Observaciones = dto.Observaciones,
                FechaCreacion = DateTime.UtcNow
            };

            //Agregar lineas
            int orden = 1;
            foreach (var lineaDto in dto.Lineas)
            {
                decimal iva = lineaDto.IVA ?? 21m;
                decimal? recargo = 0;

                if (lineaDto.ProductoId.HasValue)
                {
                    var producto = await _context.Productos.FindAsync(lineaDto.ProductoId.Value);
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
                    recargo = FiscalConfiguracionService.CalcularRecargoEquivalencia(iva);
                }

                var linea = new LineaAlbaran
                {
                    Orden = orden++,
                    Descripcion = lineaDto.Descripcion,
                    Cantidad = lineaDto.Cantidad,
                    PrecioUnitario = lineaDto.PrecioUnitario,
                    IVA = iva,
                    RecargoEquivalencia = recargo ?? 0m, // ⭐ APLICADO
                    ProductoId = lineaDto.ProductoId,
                    Importe = Math.Round(lineaDto.Cantidad * lineaDto.PrecioUnitario, 2)
                };

                albaran.Lineas.Add(linea);
            }

            CalcularTotalesAlbaran(albaran);

            _context.Albaranes.Add(albaran);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Creado albaran {numero} para tenant {TenantId}",
                numeroCompleto, tenantId);

            return await MapearAResponseDto(albaran);
            

        }

        public async Task<AlbaranResponseDto> ActualizarAlbaranAsync( int tenantId, int id, AlbaranUpdateDto dto)
        {
            var albaran = await _context.Albaranes
                .Include(a=> a.Lineas)
                .FirstOrDefaultAsync(a=> a.Id == id && a.TenantId == tenantId);

            if (albaran == null)
                throw new InvalidOperationException($"Albaran {id} no encontrado");

            //Solo se pueden editar albaranes en estado pendiente
            if (albaran.Estado != "Pendiente")
                throw new InvalidOperationException($"No se puede modificar un albarana en estado {albaran.Estado}");

            //Validar cliente
            var clienteExite = await _context.Clientes
                .AnyAsync(c => c.Id == dto.ClienteId && c.TenantId == tenantId);

            if (!clienteExite)
                throw new InvalidOperationException($"Cliente {dto.ClienteId} no encontrado");

            //Actualizar datos
            albaran.ClienteId = dto.ClienteId;
            albaran.FechaEmision = (dto.FechaEmision ?? albaran.FechaEmision).ToUniversalTime();
            albaran.FechaEntrega = dto.FechaEntrega?.ToUniversalTime();
            albaran.DireccionEntrega = dto.DireccionEntrega;
            albaran.Observaciones = dto.Observaciones;
            albaran.FechaModificacion = DateTime.UtcNow;

            //Actualizacion inteligente de lineas
            var lineasDtoIds = dto.Lineas
                .Where(l => l.Id.HasValue)
                .Select(l => l.Id.Value)
                .ToList();

            var lineasAEliminar = albaran.Lineas
                .Where(l => !lineasDtoIds.Contains(l.Id))
                .ToList();

            foreach(var lineaAEliminar in lineasAEliminar)
            {
                albaran.Lineas.Remove(lineaAEliminar);
                _context.LineasAlbaranes.Remove(lineaAEliminar);
            }

            int orden = 1;
            foreach(var lineaDto in dto.Lineas)
            {
                if(lineaDto.Id.HasValue && lineaDto.Id.Value > 0)
                {
                    //Actualizar linea existene
                    var lineaExistente = albaran.Lineas
                        .FirstOrDefault(l => l.Id == lineaDto.Id.Value);

                    if(lineaExistente!= null)
                    {
                        lineaExistente.Orden = orden++;
                        lineaExistente.Descripcion = lineaDto.Descripcion;
                        lineaExistente.Cantidad = lineaDto.Cantidad;
                        lineaExistente.PrecioUnitario = lineaDto.PrecioUnitario;
                        lineaExistente.PorcentajeDescuento = lineaDto.PorcentajeDescuento;
                        lineaExistente.IVA = lineaDto.IVA ?? lineaExistente.IVA;
                        lineaExistente.ProductoId = lineaDto.ProductoId;

                        CalcularLinea(lineaExistente);
                    }
                }
                else
                {
                    //Crear linea nueva
                    var lineaNueva = new LineaAlbaran
                    {
                        AlbaranId = albaran.Id,
                        Orden = orden++,
                        Descripcion = lineaDto.Descripcion,
                        Cantidad = lineaDto.Cantidad,
                        PrecioUnitario = lineaDto.PrecioUnitario,
                        PorcentajeDescuento=lineaDto.PorcentajeDescuento,
                        IVA = lineaDto.IVA ?? 21m,
                        ProductoId= lineaDto.ProductoId
                    };

                    CalcularLinea(lineaNueva);
                    albaran.Lineas.Add(lineaNueva);
                }
            }

            //Recalcular totales
            CalcularTotalesAlbaran(albaran);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Actualizado albaran {Numero} para Tenant {TenantId}",
                albaran.Numero, tenantId);

            return await MapearAResponseDto(albaran);
        }

        public async Task<AlbaranResponseDto> CambiarEstadoAsync(int tenantId, int id, CambiarEstadoAlbaranDto dto)
        {
            var albaran = await _context.Albaranes
                .Include(a=> a.Lineas)
                .FirstOrDefaultAsync( a=> a.Id == id && a.TenantId == tenantId);

            if (albaran == null)
                throw new InvalidOperationException($"Albaran {id} no encontrado");

            //Validar transicion de estado
            ValidarTransicionEstado(albaran.Estado,dto.NuevoEstado);

            var estadoAnterior = albaran.Estado;
            albaran.Estado = dto.NuevoEstado;
            albaran.FechaModificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cambiado estado de albaran {Numero} de {EstadoAnterior} a {EstadoNuevo}",
                albaran.Numero, estadoAnterior, dto.NuevoEstado);

            return await MapearAResponseDto(albaran);
        }


        public async Task<AlbaranResponseDto> ObtenerPorIdAsync(int tenantId, int id)
        {
            var albaran = await _context.Albaranes
                .Include(a=> a.Lineas)
                .Include(a=>a.Cliente)
                .Include(a=>a.Presupuesto)
                .FirstOrDefaultAsync(a=>a.Id == id && a.TenantId==tenantId);

            if (albaran == null) return null;

            return await MapearAResponseDto (albaran);
        }

        public async Task<List<AlbaranResponseDto>> ObtenerTodosAsync(int tenantId, string? estado = null)
        {
            var query = _context.Albaranes
                .Include(a => a.Lineas)
                .Include(a => a.Cliente)
                .Include(a => a.Presupuesto)
                .Where(a => a.TenantId == tenantId);

            if (!string.IsNullOrEmpty(estado))
            {
                query = query.Where(a => a.Estado == estado);
            }

            var albaranes =await query
            .OrderByDescending(a => a.FechaEmision)
            .ToListAsync();

            var result = new List<AlbaranResponseDto> ();
            foreach(var albaran in albaranes)
            {
                result.Add(await MapearAResponseDto(albaran));
            }

            return result;
        }


        public async Task<bool> EliminarAsync(int tenantId, int id)
        {
            var albaran = await _context.Albaranes
                .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId);

            if (albaran == null) return false;

            //Solo se pueden eliminar albaranes en estado pendiente
            if (albaran.Estado != "Pendiente")
                throw new InvalidOperationException($"No se puede eliminar un albaran en estado {albaran.Estado}");

            _context.Albaranes.Remove(albaran);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Eliminado albaran {Numero} del tenant {TenantId}",
                albaran.Id, tenantId);

            return true;
                
        }

        public async Task<AlbaranResponseDto> ConvertirDesdePresupuesto(int tenantId, int prespuestoId, ConvertirPresupuestoDto dto)
        {
            //Obtener presupuesto
            var presupuesto = await _context.Presupuestos
                .Include(p => p.Lineas)
                .Include(p => p.Cliente)
                .FirstOrDefaultAsync(p=> p.Id == prespuestoId && p.TenantId== tenantId);

            if (presupuesto == null)
                throw new InvalidOperationException($"Prespuesto {prespuestoId} no encontrado");

            //Prespuesto debe estar aceptado
            if (presupuesto.Estado != "Aceptado")
                throw new InvalidOperationException($"Solo se pueden convertir presupuestos en estado Aceptado. Estado actual: {presupuesto.Estado}");

            //Ontener serie para albaranes
            var serieAlbaran = await _context.SeriesNumeracion
                .FirstOrDefaultAsync(s =>
                s.TenantId == tenantId &&
                s.TipoDocumento == "ALBARAN" &&
                s.Activo);

            if (serieAlbaran == null)
                throw new InvalidOperationException("No hay series activas para albaranes");

            //Obtener siguiente numero
            var ejercicio = dto.FechaEmision?.Year ?? DateTime.UtcNow.Year;
            var (numeroCompleto, numero) = await _numeracionService
                .ObtenerSiguienteNumeroAsync(tenantId, serieAlbaran.Codigo, ejercicio, "ALBARAN");

            //Crear albaran
            var albaran = new Albaran
            {
                TenantId = tenantId,
                ClienteId = presupuesto.ClienteId,
                SerieId = serieAlbaran.Id,
                Numero = numeroCompleto,
                Ejercicio = ejercicio,
                FechaEmision = (dto.FechaEmision ?? DateTime.UtcNow).ToUniversalTime(),
                FechaEntrega = dto.FechaEntrega?.ToUniversalTime(),
                Estado = "Pendiente",
                DireccionEntrega = dto.DireccionEntrega ?? presupuesto.Cliente?.Direccion,
                Observaciones = dto.Observaciones ?? presupuesto.Observaciones,
                FechaCreacion = DateTime.UtcNow
            };

            //Copiar lineas del prespuesto
            var lineasACopiar = presupuesto.Lineas.AsEnumerable();

            //Filtrar lineas seleccionadas si se especifican
            if (dto.LineasSeleccionadas != null && dto.LineasSeleccionadas.Any())
            {
                lineasACopiar = lineasACopiar.Where(l => dto.LineasSeleccionadas.Contains(l.Id));
            }

            int orden = 1;
            foreach (var lineaPresupuesto in lineasACopiar.OrderBy(l => l.Orden))
            {
                var lineaAlbaran = new LineaAlbaran
                {
                    Orden = orden++,
                    Descripcion = lineaPresupuesto.Descripcion,
                    Cantidad = lineaPresupuesto.Cantidad,
                    PrecioUnitario = lineaPresupuesto.PrecioUnitario,
                    PorcentajeDescuento = lineaPresupuesto.PorcentajeDescuento,
                    ImporteDescuento = lineaPresupuesto.ImporteDescuento,
                    BaseImponible = lineaPresupuesto.BaseImponible,
                    IVA = lineaPresupuesto.IVA,
                    ImporteIva = lineaPresupuesto.ImporteIva,
                    Importe = lineaPresupuesto.Importe,
                    ProductoId = lineaPresupuesto.ProductoId
                };

                albaran.Lineas.Add(lineaAlbaran);
            }

            //Calcular totales
            CalcularTotalesAlbaran(albaran);

            _context.Albaranes.Add(albaran);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Creado albaran {NumeroAlbaran} desde presupuesto {NumeroPresupuesto} para tenant {TenantId}",
                numeroCompleto, presupuesto.Numero, tenantId);

            return await MapearAResponseDto(albaran);
        }


        #region Metodos privados

        private void CalcularLinea(LineaAlbaran linea)
        {
            var subtotal = linea.Cantidad * linea.PrecioUnitario;
            linea.ImporteDescuento = subtotal * (linea.PorcentajeDescuento / 100);
            linea.BaseImponible = subtotal - linea.ImporteDescuento;
            linea.ImporteIva = linea.BaseImponible * (linea.IVA / 100);
            linea.Importe = linea.BaseImponible + linea.ImporteIva;
        }

        private void CalcularTotalesAlbaran(Albaran albaran)
        {
            albaran.BaseImponible = albaran.Lineas.Sum(l => l.Importe);

            albaran.TotalIVA = Math.Round(
                albaran.Lineas.Sum(l => l.Importe * l.IVA / 100), 2);

            albaran.TotalRecargo = Math.Round(
                albaran.Lineas.Sum(l => l.Importe * l.RecargoEquivalencia / 100), 2);

            // Los albaranes NO tienen retención
            albaran.Total = albaran.BaseImponible + albaran.TotalIVA + albaran.TotalRecargo;
        }

        private void ValidarTransicionEstado(string estadoActual, string nuevoEstado)
        {
            var transicionesPermitidas = new Dictionary<string, List<string>>
            {
                {"Pendiente", new List<string>{"Entregado", "Anulado"} },
                {"Entregado", new List<string>{"Aceptado"} },
                {"Aceptado", new List<string>{"Facturado"} },
                {"Facturado", new List<string> ()}, //Estado final
                {"Anulado", new List<string>() } //Estado final
            };

            if (!transicionesPermitidas.ContainsKey(estadoActual))
                throw new InvalidOperationException($"Estado actual '{estadoActual}' no valido");

            if (!transicionesPermitidas[estadoActual].Contains(nuevoEstado))
                throw new InvalidOperationException($"No se puede cambiar de '{estadoActual}' a '{nuevoEstado}'");
        }

        private async Task<AlbaranResponseDto> MapearAResponseDto(Albaran albaran)
        {
            if(albaran.Cliente == null)
            {
                albaran.Cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.Id == albaran.ClienteId);
            }

            var serie = await _context.SeriesNumeracion
                .FirstOrDefaultAsync(s => s.Id == albaran.SerieId);

            return new AlbaranResponseDto
            {
                Id = albaran.Id,
                TenantId = albaran.TenantId,
                ClienteId = albaran.ClienteId,
                ClienteNombre = albaran.Cliente?.Nombre,
                SerieId = albaran.SerieId,
                SerieCodigo = serie?.Codigo,
                PresupuestoId = albaran.PresupuestoId,
                PresupuestoNumero = albaran.Presupuesto?.Numero,
                Numero = albaran.Numero,
                Ejercicio = albaran.Ejercicio,
                FechaEmision = albaran.FechaEmision,
                FechaEntrega = albaran.FechaEntrega,
                BaseImponible = albaran.BaseImponible,
                TotalIVA = albaran.TotalIVA,
                Total = albaran.Total,
                Estado = albaran.Estado,
                DireccionEntrega = albaran.DireccionEntrega,
                Observaciones = albaran.Observaciones,
                Facturado = albaran.Facturado,
                FacturaId = albaran.FacturaId,
                Lineas = albaran.Lineas.Select(l => new LineaAlbaranResponseDto
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
                    ImporteIva = l.ImporteIva,
                    Importe = l.Importe,
                    ProductoId = l.ProductoId
                }).OrderBy(l => l.Orden).ToList(),
                FechaCreacion = albaran.FechaCreacion,
                FechaModificacion = albaran.FechaModificacion
            };
        }

        #endregion
    }
}
