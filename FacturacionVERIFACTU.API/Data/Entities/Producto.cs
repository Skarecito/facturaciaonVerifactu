using API.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace FacturacionVERIFACTU.API.Data.Entities
{
    [Table("productos")]
    public class Producto
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("tenant_id")]
        public int TenantId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("codigo")]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        [Column("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [Column("precio", TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        [Column("iva", TypeName = "decimal(5,2)")]
        public decimal IVA { get; set; } = 21.00m;

        [MaxLength(20)]
        [Column("unidad")]
        public string Unidad { get; set; } = "Ud";

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Column("fecha_modificacion")]
        public DateTime FechaModificacion { get; set; } = DateTime.UtcNow;

        //Relaciones
        [ForeignKey("TenantId")]
        public Tenant Tenant { get; set; } = null;

        public ICollection<LineaPresupuesto> LineasPresupuesto { get; set; } = new List<LineaPresupuesto>();
        public ICollection<LineaAlbaran> LineasAlbaran { get; set; } = new List<LineaAlbaran>();
        public ICollection<LineaFactura> LineasFactura { get; set; } = new List<LineaFactura>();


    }
}
