using FacturacionVERIFACTU.API.Data;
using FacturacionVERIFACTU.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacturacionVERIFACTU.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            ApplicationDbContext context,
            ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Resumen general del dashboard
        /// </summary>
        [HttpGet("resumen")]
        public async Task<ActionResult<ResumenDashboardDto>> GetResumen()
        {
            try
            {
                var mesActual = DateTime.UtcNow.Month;
                var yearActual = DateTime.UtcNow.Year;
                var inicioMes = new DateTime(yearActual, mesActual, 1, 0, 0, 0, DateTimeKind.Utc);
                var finMes = inicioMes.AddMonths(1);

                // Facturación del mes actual (solo facturas emitidas, no anuladas)
                var facturacionMesActual = await _context.Facturas
                    .Where(f => f.FechaEmision >= inicioMes
                             && f.FechaEmision < finMes
                             && f.Estado == "Emitida")
                    .SumAsync(f => f.Total);

                // Facturas pendientes de pago (emitidas, no pagadas ni anuladas)
                var facturasPendientesPago = await _context.Facturas
                    .Where(f => f.Estado == "Emitida")
                    .CountAsync();

                // Presupuestos pendientes (no convertidos a factura ni rechazados)
                var presupuestosPendientes = await _context.Presupuestos
                    .Where(p => p.Estado == "Pendiente" || p.Estado == "Enviado")
                    .CountAsync();

                // Clientes con al menos una factura en los últimos 6 meses
                var seisMesesAtras = DateTime.UtcNow.AddMonths(-6);
                var clientesActivos = await _context.Facturas
                    .Where(f => f.FechaEmision >= seisMesesAtras
                             && f.Estado != "Anulada")
                    .Select(f => f.ClienteId)
                    .Distinct()
                    .CountAsync();

                var resumen = new ResumenDashboardDto
                {
                    FacturacionMesActual = facturacionMesActual,
                    FacturasPendientesPago = facturasPendientesPago,
                    PresupuestosPendientes = presupuestosPendientes,
                    ClientesActivos = clientesActivos
                };

                return Ok(resumen);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener resumen del dashboard");
                return StatusCode(500, "Error al obtener el resumen");
            }
        }

        /// <summary>
        /// Facturación mensual por año
        /// </summary>
        [HttpGet("facturacion-mensual")]
        public async Task<ActionResult<List<FacturacionMensualDto>>> GetFacturacionMensual(
            [FromQuery] int? year = null)
        {
            try
            {
                var yearConsulta = year ?? DateTime.UtcNow.Year;
                var inicioYear = new DateTime(yearConsulta, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var finYear = inicioYear.AddYears(1);

                var facturacionMensual = await _context.Facturas
                    .Where(f => f.FechaEmision >= inicioYear
                             && f.FechaEmision < finYear
                             && f.Estado != "Anulada")
                    .GroupBy(f => f.FechaEmision.Month)
                    .Select(g => new FacturacionMensualDto
                    {
                        Mes = ObtenerNombreMes(g.Key),
                        NumeroMes = g.Key,
                        Total = g.Sum(f => f.Total),
                        CantidadFacturas = g.Count()
                    })
                    .OrderBy(x => x.NumeroMes)
                    .ToListAsync();

                // Rellenar meses sin facturación
                var todosLosMeses = Enumerable.Range(1, 12)
                    .Select(m => new FacturacionMensualDto
                    {
                        Mes = ObtenerNombreMes(m),
                        NumeroMes = m,
                        Total = 0,
                        CantidadFacturas = 0
                    })
                    .ToList();

                foreach (var mes in facturacionMensual)
                {
                    var idx = todosLosMeses.FindIndex(m => m.NumeroMes == mes.NumeroMes);
                    if (idx >= 0)
                    {
                        todosLosMeses[idx] = mes;
                    }
                }

                return Ok(todosLosMeses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener facturación mensual para año {Year}", year);
                return StatusCode(500, "Error al obtener facturación mensual");
            }
        }

        /// <summary>
        /// Top clientes por facturación
        /// </summary>
        [HttpGet("clientes-top")]
        public async Task<ActionResult<List<ClienteTopDto>>> GetClientesTop(
            [FromQuery] int limit = 5,
            [FromQuery] int? year = null)
        {
            try
            {
                var query = _context.Facturas
                    .Where(f => f.Estado != "Anulada");

                // Filtrar por año si se especifica
                if (year.HasValue)
                {
                    var inicioYear = new DateTime(year.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    var finYear = inicioYear.AddYears(1);
                    query = query.Where(f => f.FechaEmision >= inicioYear
                                          && f.FechaEmision < finYear);
                }

                var clientesTop = await query
                    .GroupBy(f => new { f.ClienteId, f.Cliente.Nombre, f.Cliente.NIF })
                    .Select(g => new ClienteTopDto
                    {
                        ClienteId = g.Key.ClienteId,
                        RazonSocial = g.Key.Nombre,
                        NIF = g.Key.NIF,
                        TotalFacturado = g.Sum(f => f.Total),
                        CantidadFacturas = g.Count(),
                        UltimaFactura = g.Max(f => f.FechaEmision)
                    })
                    .OrderByDescending(c => c.TotalFacturado)
                    .Take(limit)
                    .ToListAsync();

                return Ok(clientesTop);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes top");
                return StatusCode(500, "Error al obtener clientes top");
            }
        }

        /// <summary>
        /// Productos más vendidos
        /// </summary>
        [HttpGet("productos-mas-vendidos")]
        public async Task<ActionResult<List<ProductoMasVendidoDto>>> GetProductosMasVendidos(
            [FromQuery] int limit = 10,
            [FromQuery] int? year = null)
        {
            try
            {
                var query = _context.LineasFacturas
                    .Include(lf => lf.Factura)
                    .Include(lf => lf.Producto)
                    .Where(lf => lf.Factura.Estado != "Anulada" && lf.ProductoId != null);

                // Filtrar por año si se especifica
                if (year.HasValue)
                {
                    var inicioYear = new DateTime(year.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    var finYear = inicioYear.AddYears(1);
                    query = query.Where(lf => lf.Factura.FechaEmision >= inicioYear
                                           && lf.Factura.FechaEmision < finYear);
                }

                var productosMasVendidos = await query
                    .GroupBy(lf => new
                    {
                        lf.ProductoId,
                        lf.Producto.Descripcion,
                        lf.Producto.Precio
                    })
                    .Select(g => new ProductoMasVendidoDto
                    {
                        ProductoId = g.Key.ProductoId.Value,
                        Nombre = g.Key.Descripcion,
                        CantidadVendida = g.Sum(lf => lf.Cantidad),
                        TotalFacturado = g.Sum(lf => lf.TotalLinea),
                        PrecioMedio = g.Average(lf => lf.PrecioUnitario)
                    })
                    .OrderByDescending(p => p.CantidadVendida)
                    .Take(limit)
                    .ToListAsync();

                return Ok(productosMasVendidos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos más vendidos");
                return StatusCode(500, "Error al obtener productos más vendidos");
            }
        }

        /// <summary>
        /// Evolución de facturación comparativa (año actual vs año anterior)
        /// </summary>
        [HttpGet("facturacion-comparativa")]
        public async Task<ActionResult<FacturacionComparativaDto>> GetFacturacionComparativa()
        {
            try
            {
                var yearActual = DateTime.UtcNow.Year;
                var yearAnterior = yearActual - 1;

                var inicioActual = new DateTime(yearActual, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var finActual = inicioActual.AddYears(1);
                var inicioAnterior = new DateTime(yearAnterior, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var finAnterior = inicioAnterior.AddYears(1);

                // Facturación año actual
                var facturacionActual = await _context.Facturas
                    .Where(f => f.FechaEmision >= inicioActual
                             && f.FechaEmision < finActual
                             && f.Estado != "Anulada")
                    .GroupBy(f => f.FechaEmision.Month)
                    .Select(g => new { Mes = g.Key, Total = g.Sum(f => f.Total) })
                    .ToListAsync();

                // Facturación año anterior
                var facturacionAnterior = await _context.Facturas
                    .Where(f => f.FechaEmision >= inicioAnterior
                             && f.FechaEmision < finAnterior
                             && f.Estado != "Anulada")
                    .GroupBy(f => f.FechaEmision.Month)
                    .Select(g => new { Mes = g.Key, Total = g.Sum(f => f.Total) })
                    .ToListAsync();

                var comparativa = new FacturacionComparativaDto
                {
                    YearActual = yearActual,
                    YearAnterior = yearAnterior,
                    DatosActual = Enumerable.Range(1, 12)
                        .Select(m => facturacionActual.FirstOrDefault(f => f.Mes == m)?.Total ?? 0)
                        .ToList(),
                    DatosAnterior = Enumerable.Range(1, 12)
                        .Select(m => facturacionAnterior.FirstOrDefault(f => f.Mes == m)?.Total ?? 0)
                        .ToList()
                };

                return Ok(comparativa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener facturación comparativa");
                return StatusCode(500, "Error al obtener facturación comparativa");
            }
        }

        /// <summary>
        /// Estadísticas de cobros y pendientes
        /// </summary>
        [HttpGet("estadisticas-cobros")]
        public async Task<ActionResult<EstadisticasCobrosDto>> GetEstadisticasCobros()
        {
            try
            {
                var facturas = await _context.Facturas
                    .Where(f => f.Estado != "Anulada")
                    .Select(f => new { f.Estado, f.Total })
                    .ToListAsync();

                var estadisticas = new EstadisticasCobrosDto
                {
                    TotalFacturado = facturas.Sum(f => f.Total),
                    TotalCobrado = facturas
                        .Where(f => f.Estado == "Pagada")
                        .Sum(f => f.Total),
                    TotalPendiente = facturas
                        .Where(f => f.Estado == "Emitida")
                        .Sum(f => f.Total),
                    TotalVencido = 0, // No tienes campo de vencimiento, lo dejamos en 0
                    CantidadPagadas = facturas.Count(f => f.Estado == "Pagada"),
                    CantidadPendientes = facturas.Count(f => f.Estado == "Emitida"),
                    CantidadVencidas = 0 // No tienes campo de vencimiento
                };

                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de cobros");
                return StatusCode(500, "Error al obtener estadísticas de cobros");
            }
        }

        private static string ObtenerNombreMes(int mes)
        {
            return mes switch
            {
                1 => "Enero",
                2 => "Febrero",
                3 => "Marzo",
                4 => "Abril",
                5 => "Mayo",
                6 => "Junio",
                7 => "Julio",
                8 => "Agosto",
                9 => "Septiembre",
                10 => "Octubre",
                11 => "Noviembre",
                12 => "Diciembre",
                _ => "Desconocido"
            };
        }
    }
}
