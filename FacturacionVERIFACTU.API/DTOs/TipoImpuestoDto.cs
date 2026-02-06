using System.ComponentModel.DataAnnotations;

namespace FacturacionVERIFACTU.API.DTOs
{
    public class TipoImpuestoCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Range(0, 100)]
        public decimal PorcentajeIva { get; set; }

        [Range(0, 100)]
        public decimal PorcentajeRecargo { get; set; }

        public bool Activo { get; set; } = true;
        public int? Orden { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }

    public class TipoImpuestoUpdateDto
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Range(0, 100)]
        public decimal PorcentajeIva { get; set; }

        [Range(0, 100)]
        public decimal PorcentajeRecargo { get; set; }

        public bool Activo { get; set; }
        public int? Orden { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }

    public class TipoImpuestoResponseDto
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal PorcentajeIva { get; set; }
        public decimal PorcentajeRecargo { get; set; }
        public bool Activo { get; set; }
        public int? Orden { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}
