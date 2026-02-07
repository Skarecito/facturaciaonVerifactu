using System.ComponentModel.DataAnnotations;

namespace FacturacionVERIFACTU.Web.Models.DTOs;

public class TipoImpuestoDto
{
    public int TipoImpuestoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal PorcentajeIVA { get; set; }
    public decimal? PorcentajeRecargoEquivalencia { get; set; }
    public bool Activo { get; set; }
    public bool EnUso { get; set; }
}

public class CrearTipoImpuestoDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Range(0, 100, ErrorMessage = "El IVA debe estar entre 0 y 100.")]
    public decimal PorcentajeIVA { get; set; }

    [Range(0, 100, ErrorMessage = "El recargo debe estar entre 0 y 100.")]
    public decimal? PorcentajeRecargoEquivalencia { get; set; }

    public bool Activo { get; set; } = true;
}

public class ActualizarTipoImpuestoDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Range(0, 100, ErrorMessage = "El IVA debe estar entre 0 y 100.")]
    public decimal PorcentajeIVA { get; set; }

    [Range(0, 100, ErrorMessage = "El recargo debe estar entre 0 y 100.")]
    public decimal? PorcentajeRecargoEquivalencia { get; set; }

    public bool Activo { get; set; }
}
