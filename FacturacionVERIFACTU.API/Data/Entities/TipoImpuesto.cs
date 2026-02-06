using API.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturacionVERIFACTU.API.Data.Entities
{
    [Table("tipos_impuesto")]
    public class TipoImpuesto
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("tenant_id")]
        public int TenantId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("porcentaje_iva", TypeName = "decimal(5,2)")]
        public decimal PorcentajeIva { get; set; }

        [Column("porcentaje_recargo", TypeName = "decimal(5,2)")]
        public decimal PorcentajeRecargo { get; set; }

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Column("orden")]
        public int? Orden { get; set; }

        [Column("fecha_inicio")]
        public DateTime? FechaInicio { get; set; }

        [Column("fecha_fin")]
        public DateTime? FechaFin { get; set; }

        [ForeignKey("TenantId")]
        public Tenant Tenant { get; set; } = null!;

        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
        public ICollection<LineaPresupuesto> LineasPresupuesto { get; set; } = new List<LineaPresupuesto>();
        public ICollection<LineaAlbaran> LineasAlbaran { get; set; } = new List<LineaAlbaran>();
        public ICollection<LineaFactura> LineasFactura { get; set; } = new List<LineaFactura>();
    }
}
