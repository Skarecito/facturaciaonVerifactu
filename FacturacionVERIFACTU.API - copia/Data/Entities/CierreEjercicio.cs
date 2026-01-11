// API/Data/Entities/CierreEjercicio.cs - VERSIÓN ACTUALIZADA
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
        public int TenantId { get; set; }

        [Required]
        [Column("ejercicio")]
        public int Ejercicio { get; set; } // Cambié ejercicio -> Ejercicio (PascalCase)

        [Column("fecha_cierre")]
        public DateTime FechaCierre { get; set; }

        [Required]
        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        // ========== VERIFACTU (YA EXISTÍAN) ==========
        [Required]
        [MaxLength(64)]
        [Column("hash_final")]
        public string HashFinal { get; set; } = string.Empty;

        [Column("enviado_verifactu")]
        public bool EnviadoVERIFACTU { get; set; } = false;

        [Column("fecha_envio")]
        public DateTime? FechaEnvio { get; set; }

        // ========== ESTADÍSTICAS CONTABLES (NUEVAS) ==========
        [Column("total_facturas")]
        public int TotalFacturas { get; set; }

        [Column("total_base_imponible", TypeName = "decimal(18,2)")]
        public decimal TotalBaseImponible { get; set; }

        [Column("total_iva", TypeName = "decimal(18,2)")]
        public decimal TotalIVA { get; set; }

        [Column("total_recargo", TypeName = "decimal(18,2)")]
        public decimal TotalRecargo { get; set; }

        [Column("total_retencion", TypeName = "decimal(18,2)")]
        public decimal TotalRetencion { get; set; }

        [Column("total_importe", TypeName = "decimal(18,2)")]
        public decimal TotalImporte { get; set; } // Ya existía


        // ========== ARCHIVOS GENERADOS (NUEVOS) ==========
        [Column("ruta_libro_facturas")]
        [MaxLength(500)]
        public string? RutaLibroFacturas { get; set; }

        [Column("ruta_resumen_iva")]
        [MaxLength(500)]
        public string? RutaResumenIVA { get; set; }

        // ========== CONTROL DE REAPERTURA (NUEVOS) ==========
        [Column("esta_abierto")]
        public bool EstaAbierto { get; set; } = false;

        [Column("motivo_reapertura")]
        [MaxLength(500)]
        public string? MotivoReapertura { get; set; }

        [Column("fecha_reapertura")]
        public DateTime? FechaReapertura { get; set; }

        [Column("usuario_reapertura_id")]
        public int? UsuarioReaperturaId { get; set; }

        // ========== AUDITORÍA (NUEVOS) ==========
        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        [Column("actualizado_en")]
        public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;

        // ========== RELACIONES ==========
        [ForeignKey("TenantId")]
        public Tenant Tenant { get; set; } = null!;

        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; } = null!;

        [ForeignKey("UsuarioReaperturaId")]
        public Usuario? UsuarioReapertura { get; set; }
    }
}
