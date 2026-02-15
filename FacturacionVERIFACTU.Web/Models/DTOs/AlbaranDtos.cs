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
    public List<lineaAlbaranResponseDto> Lineas { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
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
