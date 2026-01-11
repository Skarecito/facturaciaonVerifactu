using FacturacionVERIFACTU.API.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Data.Entities
{
    [Table("facturas")]
    public class Factura
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

        [Column("fecha_emision")]
        public DateTime FechaEmision { get; set; } = DateTime.UtcNow;

        [Column("base_imponible", TypeName = "decimal(18,2)")]
        public decimal BaseImponible { get; set; }

        [Column("total_recargo", TypeName = "decimal(18,2)")]
        public decimal? TotalRecargo { get; set; }

        // RETENCIÓN (a nivel de factura)
        [Column("porcentaje_retencion", TypeName = "decimal(5,2)")]
        public decimal? PorcentajeRetencion { get; set; } = 0;

        [Column("cuota_retencion", TypeName = "decimal(18,2)")]
        public decimal? CuotaRetencion { get; set; } = 0;

        [Column("total_iva", TypeName = "decimal(18,2)")]
        public decimal TotalIVA { get; set; }

        [Column("total", TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [MaxLength(20)]
        [Column("estado")]
        public string Estado { get; set; } = "Emitida"; // Emitida, Pagada, Anulada

        [MaxLength(500)]
        [Column("observaciones")]
        public string? Observaciones { get; set; }

        [Column("ejercicio")]
        public int Ejercicio { get; set; }

        // ============================================
        // CAMPOS VERIFACTU - YA EXISTENTES
        // ============================================
        [MaxLength(64)]
        [Column("huella")]
        public string? Huella { get; set; }

        [MaxLength(64)]
        [Column("huella_anterior")]
        public string? HuellaAnterior { get; set; }

        [Column("enviada_verifactu")]
        public bool EnviadaVERIFACTU { get; set; } = false;

        [Column("fecha_envio_verifactu")]
        public DateTime? FechaEnvioVERIFACTU { get; set; }

        // ============================================
        // CAMPOS VERIFACTU - NUEVOS A AGREGAR
        // ============================================
        [MaxLength(2)]
        [Column("tipo_factura_verifactu")]
        public string? TipoFacturaVERIFACTU { get; set; } // F1, F2, F3

        [Column("qr_verifactu")]
        public byte[]? QRVerifactu { get; set; } // Imagen QR en bytes

        [MaxLength(500)]
        [Column("url_verifactu")]
        public string? UrlVERIFACTU { get; set; } // URL de verificación AEAT

        [MaxLength(50)]
        [Column("numero_factura_rectificada")]
        public string? NumeroFacturaRectificada { get; set; } // Si es rectificativa

        [MaxLength(20)]
        [Column("tipo_rectificacion")]
        public string? TipoRectificacion { get; set; } // S (Sustitución), I (Diferencias)


        // NUEVOS CAMPOS PARA CIERRE
        [Column("bloqueada")]
        public bool Bloqueada { get; set; } = false;

        [Column("actualizado_en")]
        public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;

        [Column("cuota_recargo", TypeName = "decimal(18,2)")]
        public decimal CuotaRecargo { get; set; } = 0;



        // ============================================
        // RELACIONES - YA EXISTENTES
        // ============================================
        [ForeignKey("TenantId")]
        public Tenant Tenant { get; set; } = null!;

        [ForeignKey("ClienteId")]
        public Cliente Cliente { get; set; } = null!;

        [ForeignKey("SerieId")]
        public SerieNumeracion Serie { get; set; } = null!;

        public ICollection<LineaFactura> Lineas { get; set; } = new List<LineaFactura>();

        public ICollection<Albaran> Albaranes { get; set; } = new List<Albaran>();
    }
}