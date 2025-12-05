namespace FacturacionVERIFACTU.API.DTOs
{
    /// <summary>
    /// DTO para crear un nuevo producto/servicio
    /// </summary>
    public class CrearProductoDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }
        public decimal IVA { get; set; } = 21; // Por defecto 21%
        public string? Unidad { get; set; } = "Ud"; // Unidad de medida
        public bool EsServicio { get; set; } = false;
    }

    /// <summary>
    /// DTO para actualizar un producto existente
    /// </summary>
    public class ActualizarProductoDto
    {
        public string Descripcion { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }
        public decimal IVA { get; set; }
        public string? Unidad { get; set; }
        public bool EsServicio { get; set; }
    }

    /// <summary>
    /// DTO de respuesta con datos del producto
    /// </summary>
    public class ProductoResponseDto
    {
        public int ProductoId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }
        public decimal IVA { get; set; }
        public string? Unidad { get; set; }
        public bool EsServicio { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }
    }
}
