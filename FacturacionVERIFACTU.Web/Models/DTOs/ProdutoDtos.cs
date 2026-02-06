using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace FacturacionVERIFACTU.Web.Models.DTOs;

public class ProductoDto
{
    public int ProductoId { get; set; }

    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(50, ErrorMessage = "El código no puede superar 50 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(500, ErrorMessage = "La descripción no puede superar 500 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [Range(0.01, 1000000, ErrorMessage = "El precio debe ser mayor que 0.")]
    public decimal PrecioUnitario { get; set; }

    [Range(0, 100, ErrorMessage = "El IVA debe estar entre 0 y 100.")]
    public decimal IVA { get; set; } = 21;
    public decimal? RecargoEquivalenciaDefecto { get; set; }

    public string? Unidad { get; set; } = "Ud";
    public bool Activo { get; set; }
}

public class CrearProductoDto
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(50, ErrorMessage = "El código no puede superar 50 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(500, ErrorMessage = "La descripción no puede superar 500 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [Range(0.01, 1000000, ErrorMessage = "El precio debe ser mayor que 0.")]
    public decimal PrecioUnitario { get; set; }

    [Range(0, 100, ErrorMessage = "El IVA debe estar entre 0 y 100.")]
    public decimal IVA { get; set; } = 21;

    public decimal? RecargoEquivalenciaDefecto { get; set; }
    public string? Unidad { get; set; } = "Ud";
    public bool Activo { get; set; }
}

public class ActualizarProductoDto
{
    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(500, ErrorMessage = "La descripción no puede superar 500 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [Range(0.01, 1000000, ErrorMessage = "El precio debe ser mayor que 0.")]
    public decimal PrecioUnitario { get; set; }

    [Range(0, 100, ErrorMessage = "El IVA debe estar entre 0 y 100.")]
    public decimal IVA { get; set; }

    public decimal? RecargoEquivalenciaDefecto { get; set; }
    public string? Unidad { get; set; }
    public bool Activo { get; set; }
}
