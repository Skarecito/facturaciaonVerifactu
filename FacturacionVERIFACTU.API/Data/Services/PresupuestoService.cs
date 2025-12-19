using FacturacionVERIFACTU.API.DTOs;
using FacturacionVERIFACTU.API.Data.Entities;
using FacturacionVERIFACTU.API.Data.Interfaces;
using FacturacionVERIFACTU.API.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

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
            //Validar cliente existe y pertenece al tenant
            var clienteExiste = await _context.Clientes
                .AnyAsync(c=>c.Id == dto.ClienteId && c.TenantId == tenantId);

            if (!clienteExiste)
            {
                throw new InvalidOperationException($"Cliente {dto.ClienteId} no encontrado");
            }

            //Obtener siguiente numero
            var ejercicio = dto.Fecha?.Year ?? DateTime.UtcNow.Year;

            // Obtener el código de la serie desde la BD
            var serie = await _context.SeriesNumeracion
                .FirstOrDefaultAsync(s => s.Id == dto.SerieId && s.TenantId == tenantId);

            if (serie == null)
            {
                throw new InvalidOperationException($"Serie {dto.SerieId} no encontrada");
            }

            // Obtener siguiente número
            var (numeroCompleto, numero) = await _numeracionService
                .ObtenerSiguienteNumeroAsync(tenantId, serie.Codigo, ejercicio);

            // Crear presupuesto
            var presupuesto = new Presupuesto
            {
                TenantId = tenantId,
                ClienteId = dto.ClienteId,
                SerieId = dto.SerieId,
                Numero = numeroCompleto,     // "P2024-001"
                Ejercicio = ejercicio,        // 2024
                Fecha = (dto.Fecha ?? DateTime.UtcNow).ToUniversalTime(),
                FechaValidez = (dto.FechaValidez ?? DateTime.UtcNow.AddDays(30)).ToUniversalTime(),
                Estado = "Borrador",
                Observaciones = dto.Observaciones,
                FechaCreacion = DateTime.UtcNow
            };

            //Agregar lineas
            int orden = 1;
            foreach(var lineaDto in dto.Lineas)
            {
                var linea = new LineaPresupuesto
                {
                    Orden = orden++,
                    Descripcion = lineaDto.Descripcion,
                    Cantidad = lineaDto.Cantidad,
                    PrecioUnitario = lineaDto.PrecioUnitario,
                    PorcentajeDescuento = lineaDto.PorcentajeDescuento,
                    IVA = lineaDto.PorcentajeIVA,
                    ProductoId = lineaDto.ArticuloId
                };

                CalcularLinea(linea);
                presupuesto.Lineas.Add(linea);
            }

            //Calcular totales
            CalcularTotalesPrespuesto(presupuesto);

            _context.Presupuestos.Add(presupuesto);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Creado presupuesto {NumeroPresupuesto} para tenant {TenantId}",
                numeroCompleto, tenantId);

            return await MapearAResponseDto(presupuesto);
        }


        public async Task<PresupuestoResponseDto> ActualizarPresupuestoAsync(
            int tenantId,
            int id,
            PresupuestoUpdateDto dto)
        {
            var presupuesto = await _context.Presupuestos
                .Include(p=>p.Lineas)
                .FirstOrDefaultAsync(p=> p.Id == id && p.TenantId == tenantId);

            if(presupuesto == null)
            {
                throw new InvalidOperationException($"Prespuesto {id} no encontrado");
            }

            //Solo se pueden editar prespuestos en estado Borrador
            if (presupuesto.Estado != "Borrador")
            {
                throw new InvalidOperationException($"No se puede modificar un presupuesto en estado {presupuesto.Estado}");
            }

            //Validar cliente
            var clienteExite = await _context.Clientes
                .AnyAsync(c=> c.Id == dto.ClienteId && c.TenantId == tenantId);

            if (!clienteExite)
            {
                throw new InvalidOperationException($"Cliente {dto.ClienteId} no encontrado");
            }

            //Actualizar datos
            presupuesto.ClienteId = dto.ClienteId;
            presupuesto.Fecha = (dto.FechaEmision ?? presupuesto.Fecha).ToUniversalTime();
            presupuesto.FechaValidez = (dto.FechaValidez ?? presupuesto.FechaValidez).ToUniversalTime();
            presupuesto.Observaciones = dto.Observaciones;
            presupuesto.FechaModificacion = DateTime.UtcNow;

            //Obtener ids de las lineas
            var lineasDtoIds = dto.Lineas
                .Where(l => l.Id.HasValue)
                .Select(l => l.Id.Value)
                .ToList();

            //Eliminar las lineas que ya no existen en la base de datos
            var lineasAEliminar = presupuesto.Lineas
                .Where(l => !lineasDtoIds.Contains(l.Id))
                .ToList();

            foreach ( var lineaAEliminar in lineasAEliminar)
            {
                presupuesto.Lineas.Remove(lineaAEliminar);
                _context.LineasPresupuesto.Remove(lineaAEliminar);
            }

            //Actualiar o crear lineas
            int orden = 1;
            foreach(var lineaDto in dto.Lineas)
            {
                if (lineaDto.Id.HasValue && lineaDto.Id.Value > 0)
                {
                    //Actualizar lina existente
                    var lineaExistente = presupuesto.Lineas
                        .FirstOrDefault(l => l.Id == lineaDto.Id.Value);
                    if (lineaExistente != null)
                    {
                        lineaExistente.Orden = orden++;
                        lineaExistente.Descripcion = lineaDto.Descripcion;
                        lineaExistente.Cantidad = lineaDto.Cantidad;
                        lineaExistente.PrecioUnitario = lineaDto.PrecioUnitario;
                        lineaExistente.PorcentajeDescuento = lineaDto.PorcentajeDescuento;
                        lineaExistente.IVA = lineaDto.PorcentajeIVA;
                        lineaExistente.ProductoId = lineaDto.ArticuloId;

                        CalcularLinea(lineaExistente);
                    }
                }
                else
                {
                    //Crear linea nueva
                    var lineaNueva = new LineaPresupuesto
                    {
                        PresupuestoId = presupuesto.Id,
                        Orden = orden++,
                        Descripcion = lineaDto.Descripcion,
                        Cantidad = lineaDto.Cantidad,
                        PrecioUnitario  = lineaDto.PrecioUnitario,
                        PorcentajeDescuento = lineaDto.PorcentajeDescuento,
                        IVA= lineaDto.PorcentajeIVA,
                        ProductoId = lineaDto.ArticuloId
                    };
                    CalcularLinea(lineaNueva);
                    presupuesto.Lineas.Add(lineaNueva);
                }
            }

            //Recalcular totales
            CalcularTotalesPrespuesto(presupuesto);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Actualizacion presupuesto {NumeroPresupuesto} para tenant {TenantId}", presupuesto.Numero, tenantId);

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
            var subTotal = linea.Cantidad * linea.PrecioUnitario;

            //Descuento
            linea.ImporteDescuento = subTotal * (linea.PorcentajeDescuento / 100);

            //Base imponible = Subtotal - Descuento
            linea.BaseImponible = subTotal - linea.ImporteDescuento;

            //Iva
            linea.ImporteIva = linea.BaseImponible * (linea.IVA / 100);

            //Total linea
            linea.Importe = linea.BaseImponible + linea.ImporteIva;
        }

        ///<summary>
        ///Calcula los totales del presupuesto
        /// </summary>
        private void CalcularTotalesPrespuesto(Presupuesto presupuesto)
        {
            presupuesto.BaseImponible = presupuesto.Lineas.Sum(l => l.BaseImponible);
            presupuesto.TotalIva = presupuesto.Lineas.Sum(l => l.ImporteIva);
            presupuesto.Total = presupuesto.Lineas.Sum(l => l.Importe);
        }

        /// <summary>
        /// Valida que la transición de estado sea permitida
        /// </summary>
        private void ValidarTransicionEstado(string estadoActual, string nuevoEstado)
        {
            var transicionesPermitidas = new Dictionary<string, List<string>>
            {
                { "Borrador", new List<string> { "Enviado" } },
                { "Enviado", new List<string> { "Aceptado", "Rechazado" } },
                { "Aceptado", new List<string>() }, // Estado final
                { "Rechazado", new List<string>() }  // Estado final
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
                    PorcentajeIVA = l.IVA,
                    ImporteIVA = l.ImporteIva,
                    Total = l.Importe,
                    ArticuloId = l.ProductoId
                }).ToList(),
                FechaCreacion = presupuesto.FechaCreacion,
                FechaModificacion = presupuesto.FechaModificacion
            };
        }

        #endregion
    }
}

