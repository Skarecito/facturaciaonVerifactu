using API.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturacionVERIFACTU.API.Data.Entities
{
    [Table("clientes")]
    public class Cliente
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("tenant_id")]
        public int TenantId {  get; set; }

        [Required]
        [MaxLength(200)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [Column("nif")]
        public string NIF { get; set; } = string.Empty;

        [MaxLength(200)]
        [Column("direccion")]
        public string? Direccion {  get; set; }

        [MaxLength(100)]
        [Column("ciudad")]
        public string? Ciudad {  get; set; }

        [MaxLength(50)]
        [Column("codigo_postal")]
        public string? CodigoPostal {  get; set; }

        [MaxLength(50)]
        [Column("provincia")]
        public string? Provincia {  get; set; }

        [MaxLength(100)]
        [Column("pais")]
        public string? Pais { get; set; }

        [MaxLength(100)]
        [Column("email")]
        public string? Email {  get; set; }

        [MaxLength(20)]
        [Column("telefono")]
        public string? Telefono {  get; set; }

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Column("fecha_modificaion")]
        public DateTime FechaModificacion { get; set; } = DateTime.UtcNow;

        //Relaciones
        [ForeignKey("TenantId")]
        public Tenant Tenant { get; set; } = null;

        public ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();
        public ICollection<Albaran> Albaranes { get; set; } = new List<Albaran>();
        public ICollection<Factura> Facturas { get; set; } = new List<Factura>();
    }
}
