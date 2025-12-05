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
        public int TenantId {  get; set; }

        [Required]
        [Column("cliente_id")]
        public int ClienteId {  get; set; }

        [Required]
        [Column("serie_id")]
        public int SerieId {  get; set; }

        [Required]
        [MaxLength(50)]
        [Column("numero")]
        public string Numero { get; set; } = string.Empty;

        [Column("fecha")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [Column("fecha_validez")]
        public DateTime FechaValidez { get; set; }

        [Column("base_imponible",TypeName ="decimal(18,2)")]
        public decimal BaseImponible {  get; set; }

        [Column("total_iva",TypeName ="decimal(18,2)")]
        public decimal TotalIva {  get; set; }

        [Column("total",TypeName ="decimal(28,2)")]
        public decimal Total {  get; set; }

        [MaxLength(20)]
        [Column("estado")]
        public string Estado { get; set; } = "Borrador"; //Borrador, enviado, aceptado

        [MaxLength(500)]
        [Column("observaciones")]
        public string? Observaciones {  get; set; }


        //Relaciones
        [ForeignKey("TenantId")]
        public Tenant Tenant { get; set; } = null;

        [ForeignKey("ClienteId")]
        public Cliente Cliente { get; set; } = null;

        public ICollection<LineaPrespuesto> Lineas { get; set; } = new List<LineaPrespuesto>();
        public ICollection<Albaran> Albaranes { get; set; } = new List<Albaran>();
    }
}
