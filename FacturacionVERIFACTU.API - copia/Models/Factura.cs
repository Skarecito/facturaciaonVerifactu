using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturacionVERIFACTU.API.Models
{
    [Table("facturas")]
    public class Factura
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("empresa_id")]
        public Guid EmpresaId { get; set; }

        [Required]
        [Column("numero_factura")]
        [MaxLength(50)]
        public string NumeroFactura { get; set; } = string.Empty;

        [Required]
        [Column("serie")]
        [MaxLength(20)]
        public string Serie { get; set; } = string.Empty;

        [Required]
        [Column("fecha_expedicion")]
        public DateTime FechaExpedicion { get; set; }

        [Column("fecha_operacion")]
        public DateTime? FechaOperacion { get; set; }

        [Required]
        [Column("base_imponible", TypeName = "decimal(12,2)")]
        public decimal BaseImponible { get; set; }

        [Required]
        [Column("tipo_iva", TypeName = "decimal(5,2)")]
        public decimal TipoIva { get; set; }

        [Required]
        [Column("cuota_iva", TypeName = "decimal(12,2)")]
        public decimal CuotaIva { get; set; }

        [Required]
        [Column("total", TypeName = "decimal(12,2)")]
        public decimal Total { get; set; }

        [Column("cliente_nif")]
        [MaxLength(20)]
        public string? ClienteNif { get; set; }

        [Column("cliente_nombre")]
        [MaxLength(255)]
        public string? ClienteNombre { get; set; }

        [Column("hash_verifactu")]
        [MaxLength(255)]
        public string? HashVerifactu { get; set; }

        [Column("qr_code")]
        public string? QrCode { get; set; }

        [Column("fecha_envio_verifactu")]
        public DateTime? FechaEnvioVerifactu { get; set; }

        [Column("estado_verifactu")]
        [MaxLength(50)]
        public string EstadoVerifactu { get; set; } = "pendiente";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navegación
        [ForeignKey("EmpresaId")]
        public Empresa Empresa { get; set; } = null!;

        public ICollection<LineaFactura> Lineas { get; set; } = new List<LineaFactura>();
    }
}