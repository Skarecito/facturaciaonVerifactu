namespace FacturacionVERIFACTU.Web.Models.DTOs
{
    public class ResumenDashboardDto
    {
        public decimal FacturacionMesActual { get; set; }
        public int FacturasPendientesPago { get; set; }
        public int PresupuestosPendientes { get; set; }
        public int ClientesActivos { get; set; }
    }

    public class FacturacionMensualDto
    {
        public string Mes { get; set; } = string.Empty;
        public int NumeroMes { get; set; }
        public decimal Total { get; set; }
        public int CantidadFacturas { get; set; }
    }

    public class ClienteTopDto
    {
        public int ClienteId { get; set; }
        public string RazonSocial { get; set; } = string.Empty;
        public string NIF { get; set; } = string.Empty;
        public decimal TotalFacturado { get; set; }
        public int CantidadFacturas { get; set; }
        public DateTime? UltimaFactura { get; set; }
    }

    public class ProductoMasVendidoDto
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal CantidadVendida { get; set; }
        public decimal TotalFacturado { get; set; }
        public decimal PrecioMedio { get; set; }
    }

    public class FacturacionComparativaDto
    {
        public int YearActual { get; set; }
        public int YearAnterior { get; set; }
        public List<decimal> DatosActual { get; set; } = new();
        public List<decimal> DatosAnterior { get; set; } = new();
    }

    public class EstadisticasCobrosDto
    {
        public decimal TotalFacturado { get; set; }
        public decimal TotalCobrado { get; set; }
        public decimal TotalPendiente { get; set; }
        public decimal TotalVencido { get; set; }
        public int CantidadPagadas { get; set; }
        public int CantidadPendientes { get; set; }
        public int CantidadVencidas { get; set; }
    }
}

