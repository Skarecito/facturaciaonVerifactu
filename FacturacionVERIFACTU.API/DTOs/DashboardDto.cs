namespace FacturacionVERIFACTU.API.DTOs
{
    /// <summary>
    /// DTO para el resumen general del dashboard
    /// </summary>
    public class ResumenDashboardDto
    {
        /// <summary>
        /// Total facturado en el mes actual
        /// </summary>
        public decimal FacturacionMesActual { get; set; }

        /// <summary>
        /// Número de facturas pendientes de pago
        /// </summary>
        public int FacturasPendientesPago { get; set; }

        /// <summary>
        /// Número de presupuestos pendientes
        /// </summary>
        public int PresupuestosPendientes { get; set; }

        /// <summary>
        /// Número de clientes activos (con facturas en los últimos 6 meses)
        /// </summary>
        public int ClientesActivos { get; set; }
    }

    /// <summary>
    /// DTO para facturación mensual
    /// </summary>
    public class FacturacionMensualDto
    {
        /// <summary>
        /// Nombre del mes
        /// </summary>
        public string Mes { get; set; } = string.Empty;

        /// <summary>
        /// Número del mes (1-12)
        /// </summary>
        public int NumeroMes { get; set; }

        /// <summary>
        /// Total facturado en el mes
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Cantidad de facturas emitidas
        /// </summary>
        public int CantidadFacturas { get; set; }
    }

    /// <summary>
    /// DTO para clientes top
    /// </summary>
    public class ClienteTopDto
    {
        /// <summary>
        /// ID del cliente
        /// </summary>
        public int ClienteId { get; set; }

        /// <summary>
        /// Razón social del cliente
        /// </summary>
        public string RazonSocial { get; set; } = string.Empty;

        /// <summary>
        /// NIF del cliente
        /// </summary>
        public string NIF { get; set; } = string.Empty;

        /// <summary>
        /// Total facturado al cliente
        /// </summary>
        public decimal TotalFacturado { get; set; }

        /// <summary>
        /// Cantidad de facturas emitidas
        /// </summary>
        public int CantidadFacturas { get; set; }

        /// <summary>
        /// Fecha de la última factura
        /// </summary>
        public DateTime UltimaFactura { get; set; }
    }

    /// <summary>
    /// DTO para productos más vendidos
    /// </summary>
    public class ProductoMasVendidoDto
    {
        /// <summary>
        /// ID del producto
        /// </summary>
        public int ProductoId { get; set; }

        /// <summary>
        /// Nombre del producto
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Referencia del producto
        /// </summary>
        public string Referencia { get; set; } = string.Empty;

        /// <summary>
        /// Cantidad total vendida
        /// </summary>
        public decimal CantidadVendida { get; set; }

        /// <summary>
        /// Total facturado del producto
        /// </summary>
        public decimal TotalFacturado { get; set; }

        /// <summary>
        /// Precio medio de venta
        /// </summary>
        public decimal PrecioMedio { get; set; }
    }

    /// <summary>
    /// DTO para facturación comparativa (año actual vs año anterior)
    /// </summary>
    public class FacturacionComparativaDto
    {
        /// <summary>
        /// Año actual
        /// </summary>
        public int YearActual { get; set; }

        /// <summary>
        /// Año anterior
        /// </summary>
        public int YearAnterior { get; set; }

        /// <summary>
        /// Datos de facturación del año actual (12 valores, uno por mes)
        /// </summary>
        public List<decimal> DatosActual { get; set; } = new();

        /// <summary>
        /// Datos de facturación del año anterior (12 valores, uno por mes)
        /// </summary>
        public List<decimal> DatosAnterior { get; set; } = new();
    }

    /// <summary>
    /// DTO para estadísticas de cobros
    /// </summary>
    public class EstadisticasCobrosDto
    {
        /// <summary>
        /// Total facturado (todas las facturas)
        /// </summary>
        public decimal TotalFacturado { get; set; }

        /// <summary>
        /// Total cobrado (facturas pagadas)
        /// </summary>
        public decimal TotalCobrado { get; set; }

        /// <summary>
        /// Total pendiente de cobro
        /// </summary>
        public decimal TotalPendiente { get; set; }

        /// <summary>
        /// Total vencido (facturas con pago vencido)
        /// </summary>
        public decimal TotalVencido { get; set; }

        /// <summary>
        /// Cantidad de facturas pagadas
        /// </summary>
        public int CantidadPagadas { get; set; }

        /// <summary>
        /// Cantidad de facturas pendientes
        /// </summary>
        public int CantidadPendientes { get; set; }

        /// <summary>
        /// Cantidad de facturas vencidas
        /// </summary>
        public int CantidadVencidas { get; set; }

        /// <summary>
        /// Porcentaje de cobro (calculado automáticamente)
        /// </summary>
        public decimal PorcentajeCobro => TotalFacturado > 0
            ? Math.Round((TotalCobrado / TotalFacturado) * 100, 2)
            : 0;
    }
}
