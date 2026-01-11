using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturacionVERIFACTU.API.Data.Entities
{
    [Table("usuarios")]
    public class Usuario
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

        // ⚠️ ALIAS PARA AUTHCONTROLLER ⚠️
        [NotMapped]
        public string NombreCompleto
        {
            get => Nombre;
            set => Nombre = value;
        }

        [Required]
        [MaxLength(100)]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [Column("rol")]
        public string Rol { get; set; } = "Usuario"; // ⚠️ Corregido typo "Usurio"

        // ⚠️ ALIAS PARA AUTHCONTROLLER ⚠️
        [NotMapped]
        public string Role
        {
            get => Rol;
            set => Rol = value;
        }

        [Column("activo")]
        public bool Activo { get; set; } = true; // ⚠️ Corregido typo "ACtivo"

        [Column("ultimo_acceso")]
        public DateTime? UltimoAcceso { get; set; }

        [Column("fecha_creaccion")]
        public DateTime FechaCreaccion { get; set; } = DateTime.UtcNow;

        // ⚠️ ALIAS PARA AUTHCONTROLLER ⚠️
        [NotMapped]
        public DateTime FechaCreacion
        {
            get => FechaCreaccion;
            set => FechaCreaccion = value;
        }

        // Relaciones
        [ForeignKey("TenantId")]
        public Tenant Tenant { get; set; } = null!;
    }
}