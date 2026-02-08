using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace FacturacionVERIFACTU.Web.Models.DTOs;

public class ProductoDto
{
    public int ProductoId { get; set; }

    public int? TipoImpuestoId { get; set; }
    public string? TipoImpuestoNombre {  get; set; }

    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(50, ErrorMessage = "El código no puede superar 50 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(500, ErrorMessage = "La descripción no puede superar 500 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [Range(0.01, 1000000, ErrorMessage = "El precio debe ser mayor que 0.")]
    public decimal PrecioUnitario { get; set; }

    public decimal? PorcentajeIva { get; set; }

    public string? Unidad { get; set; } = "Ud";
    public bool Activo { get; set; }
}

public class CrearProductoDto
{
    [Required(ErrorMessage = "Selecciona un tipo de impuesto.")]
    public int? TipoImpuestoId { get; set; }

    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(50, ErrorMessage = "El código no puede superar 50 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(500, ErrorMessage = "La descripción no puede superar 500 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [Range(0.01, 1000000, ErrorMessage = "El precio debe ser mayor que 0.")]
    public decimal PrecioUnitario { get; set; }

    public string? Unidad { get; set; } = "Ud";
    public bool Activo { get; set; }
}

public class ActualizarProductoDto
{
    [Required(ErrorMessage = "Selecciona un tipo de impuesto.")]
    public int? TipoImpuestoId { get; set; }

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(500, ErrorMessage = "La descripción no puede superar 500 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [Range(0.01, 1000000, ErrorMessage = "El precio debe ser mayor que 0.")]
    public decimal PrecioUnitario { get; set; }

    public string? Unidad { get; set; }
    public bool Activo { get; set; }
}
