using FacturacionVERIFACTU.API.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Data.Entities
{
    [Table("albaranes")]
    public class Albaran
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

        [Column("presupuesto_id")]
        public int? PresupuestoId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("numero")]
        public string Numero { get; set; } = string.Empty;

        [Column("fecha_emision")]
        public DateTime FechaEmision { get; set; } = DateTime.UtcNow;

        [Column("fecha_entrega")]
        public DateTime? FechaEntrega { get; set; }

        [Column("base_imponible", TypeName = "decimal(18,2)")]
        public decimal BaseImponible { get; set; }

        [Column("total_iva", TypeName = "decimal(18,2)")]
        public decimal TotalIVA { get; set; }

        [Column("total", TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [MaxLength(20)]
        [Column("estado")]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Entregado, Facturado, Anulado

        [MaxLength(200)]
        [Column("direccion_entrega")]
        public string? DireccionEntrega { get; set; }

        [MaxLength(500)]
        [Column("observaciones")]
        public string? Observaciones { get; set; }

        [Column("facturado")]
        public bool Facturado { get; set; } = false;

        [Column("factura_id")]
        public int? FacturaId { get; set; }

        [Column("total_recargo", TypeName = "decimal(18,2)")]
        public decimal TotalRecargo { get; set; }

        // Relaciones
        [ForeignKey("TenantId")]
        public Tenant Tenant { get; set; } = null!;

        [ForeignKey("ClienteId")]
        public Cliente Cliente { get; set; } = null!;

        [ForeignKey("SerieId")]
        public SerieNumeracion Serie { get; set; } = null!;

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Column("fecha_modificacion")]
        public DateTime? FechaModificacion { get; set; }

        [Column("ejercicio")]
        public int Ejercicio { get; set; }

        [ForeignKey("PresupuestoId")]
        public Presupuesto? Presupuesto { get; set; }

        [ForeignKey("FacturaId")]
        public Factura? Factura { get; set; }

        public ICollection<LineaAlbaran> Lineas { get; set; } = new List<LineaAlbaran>();
    }
}
