using API.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturacionVERIFACTU.API.Data.Entities
{
    [Table("presupuestos")]
    public class Presupuesto
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("tenant_id")]
        public int TenantId { get; set; }

        [Required]
        [Column("cliente_id")]
        public int ClienteId { get; set; }

        [Required]
        [Column("serie_id")]
        public int SerieId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("numero")]
        public string Numero { get; set; } = string.Empty;

        // CAMPO ADICIONAL: ejercicio (año del presupuesto)
        [Column("ejercicio")]
        public int Ejercicio { get; set; }

        [Column("fecha")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [Column("fecha_validez")]
        public DateTime FechaValidez { get; set; }

        [Column("base_imponible", TypeName = "decimal(18,2)")]
        public decimal BaseImponible { get; set; }

        [Column("total_iva", TypeName = "decimal(18,2)")]
        public decimal TotalIva { get; set; }

        [Column("total", TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [MaxLength(20)]
        [Column("estado")]
        public string Estado { get; set; } = "Borrador"; // Borrador, Enviado, Aceptado, Rechazado

        [MaxLength(500)]
        [Column("observaciones")]
        public string? Observaciones { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Column("fecha_modificacion")]
        public DateTime? FechaModificacion { get; set; }
       
        [Column("factura_id")]
        public int? FacturaId { get; set; }

        // API/Data/Entities/Presupuesto.cs - AGREGAR CAMPOS
        [Column("total_recargo", TypeName = "decimal(18,2)")]
        public decimal? TotalRecargo { get; set; }

        [Column("porcentaje_retencion", TypeName = "decimal(5,2)")]
        public decimal? PorcentajeRetencion { get; set; } = 0;

        [Column("cuota_retencion", TypeName = "decimal(18,2)")]
        public decimal? CuotaRetencion { get; set; } = 0;

        // Propiedad calculada para mostrar en presupuesto
        [NotMapped]
        public decimal? TotalConRetencion => Total - CuotaRetencion;


        //Relaciones
        [ForeignKey("TenantId")]
        public Tenant Tenant { get; set; } = null;

        [ForeignKey("ClienteId")]
        public Cliente Cliente { get; set; } = null;
       
        [ForeignKey("SerieId")]
        public SerieNumeracion Serie { get; set; } = null!;

        public ICollection<LineaPresupuesto> Lineas { get; set; } = new List<LineaPresupuesto>();
        public ICollection<Albaran> Albaranes { get; set; } = new List<Albaran>();
    }
}
