using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturacionVERIFACTU.API.Data.Entities
{
    [Table("cierres_ejercicios")]
    public class CierreEjercicio
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("tenant_id")]
        public int TenantId {  get; set; }

        [Column("ejercicio")]
        public int ejercicio {  get; set; }

        [Column("fecha_cierre")]
        public DateTime FechaCierre {  get; set; }

        [Required]
        [MaxLength(64)]
        [Column("hash_final")]
        public string HashFinal { get; set; } = string.Empty;

        [Column("total_facturas")]
        public int TotalFacturas {  get; set; }

        [Column("total_importe",TypeName ="decimal(28,2)")]
        public decimal TotalImporte {  get; set; }

        [Column("enviado_verifactu")]
        public bool EnviadoVERIFACTU { get; set; } = false;

        [Column("fecha_envio")]
        public DateTime? FechaEnvio { get; set; }

        //Relaciones
        [ForeignKey("TenantId")]
        public Tenant Tenant { get; set; }
    }
}
