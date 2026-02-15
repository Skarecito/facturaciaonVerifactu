using System.ComponentModel.DataAnnotations;

namespace FacturacionVERIFACTU.Web.Models.DTOs;

public class AlbaranResponseDto
{
    public int Id { get; set; }
    public int TenantId {  get; set; }
    public string NumeroAlbaran { get; set; } = string.Empty;
    public int SerieId {  get; set; }
    public int Numero {  get; set; }
    public int Ejercicio { get; set; }
    public DateTime FechaEmision {  get; set; }
    public DateTime? FechaEntrega {  get; set; }
    public int ClienteId { get; set;}
    public string? ClienteNombre { get; set; }
    public string Estado {  get; set; }
    public decimal BaseImponible {  get; set; }
    public decimal TotalIVA {  get; set; }
    public decimal Total {  get; set; }
    public string? Observaciones {  get; set; }
    public decimal TotalRecargo {  get; set; }
    public decimal PorcentajeRetencion { get; set; }
    public decimal CuotaRetencion { get; set; }
    public decimal TotalConRetencion { get; set; }
    public bool Facturado { get; set; }
    public List<LineaAlbaranResponseDto> Lineas { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}

public class AlbaranCreateDto
{
    [Required]
    public int ClienteId { get; set; }
    [Required] 
    public int SerieId {  get; set; }
    public DateTime? Fecha { get; set; }
    public DateTime? FechaEntrega {  get; set; }
    [StringLength(500)]
    public string? Observaciones { get; set; }
    public decimal? PorcentajeRetencion { get; set; }
    [Required]
    public List<LineaAlbaranDto> Lineas { get; set; } = new();
}

public class AlbaranUpdateDto
{
    [Required]
    public int ClienteId { get; set; }

    public DateTime? FechaEmision {  get; set; }
    public DateTime? FechaEnvio { get; set; }
    [StringLength(500)]
    public string? Observaciones { get; set; }

    public decimal? PorcentajeRetencion { get; set; }

    [Required]
    public List<LineaAlbaranDto> Lineas { get; set; } = new();
}

public class LineaAlbaranDto
{
    public int? Id { get; set; }
    public int? TipoImpuestoId { get; set; }
    
    [Required]
    [StringLength(500)]
    public string Descripcion { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Cantidad { get; set; }

    [Required]
    [Range(0,double.MaxValue)]
    public decimal PrecioUnitario { get; set; }

    [Range(0,100)]
    public decimal PorcentajeDescuento {  get; set; }

    [Range(0,100)]
    public decimal? IVA { get; set; }

    public decimal? RecargoEquivalencia { get; set; }
    public int? ArticuloId { get; set; }
    public string? ArticuloCodigo {  get; set; }

}

public class LineaAlbaranResponseDto
{
    public int Id { get; set; }
    public int Orden { get; set; }
    public int? TipoImpuestoId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal PorcentajeDescuento { get; set; }
    public decimal ImporteDescuento { get; set; }
    public decimal BaseImponible { get; set; }
    public decimal IVA { get; set; }
    public decimal ImporteIVA { get; set; }
    public decimal RecargoEquivalencia { get; set; }
    public decimal ImporteRecargo { get; set; }
    public decimal Total { get; set; }
    public int? ArticuloId { get; set; }
    public string? ArticuloCodigo { get; set; }
}

public class CambiarEstadoAlbaranDto
{
    [Required]
    [StringLength(20)]
    public string NuevoEstado { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Motivo { get; set; }
}

public class ConvertirAlbaranAFacturaDto
{
    [Required]
    public int SerieId { get; set; }

    public DateTime? FechaEmision { get; set; }

    [MaxLength(500)]
    public string? Observaciones { get; set; }


    public List<int>? LineasSeleccionadas { get; set; }

    public List<ModificarLineaDto>? LineasModificadas { get; set; }

    [Range(0, 100)]
    public decimal? PorcentajeRetencion { get; set; }
}



public class ConvertirPresupuestoAAlbaranDto
{
    public int SerieId {  get; set; }
    public DateTime? FechaEmision { get; set; }

    public DateTime? FechaEntrega { get; set; }

    [MaxLength(200)]
    public string? DireccionEntrega { get; set; }

    [MaxLength(500)]
    public string? Observaciones { get; set; }

    public List<int>? LineasSeleccionadas { get; set; }
}
