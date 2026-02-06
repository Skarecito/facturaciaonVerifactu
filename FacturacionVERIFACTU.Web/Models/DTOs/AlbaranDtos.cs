using System.ComponentModel.DataAnnotations;

namespace FacturacionVERIFACTU.Web.Models.DTOs;

public class AlbaranResponseDto
{
    public int Id { get; set; }
    public string? Numero { get; set; }
    public string? Estado { get; set; }
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
