using FacturacionVERIFACTU.API.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturacionVERIFACTU.API.Data.Entities
{
    public class Tenant
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        // ⚠️ ALIAS AÑADIDO PARA COMPATIBILIDAD CON AUTHCONTROLLER ⚠️
        [NotMapped]
        public string NombreEmpresa
        {
            get => Nombre;
            set => Nombre = value;
        }

        [Required]
        [MaxLength(20)]
        [Column("nif")]
        public string NIF { get; set; } = string.Empty;

        [MaxLength(200)]
        [Column("direccion")]
        public string? Direccion { get; set; }

        [MaxLength(100)]
        [Column("ciudad")]
        public string? Ciudad { get; set; }

        [MaxLength(20)]
        [Column("codigo_postal")]
        public string? CodigoPostal { get; set; }

        [MaxLength(50)]
        [Column("provincia")]
        public string? Provincia { get; set; }

        [MaxLength(100)]
        [Column("email")]
        public string? Email { get; set; }

        [MaxLength(20)]
        [Column("telefono")]
        public string? Telefono { get; set; }

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Column("fecha_alta")]
        public DateTime FechaAlta { get; set; } = DateTime.UtcNow;

        // ⚠️ ALIAS AÑADIDO PARA COMPATIBILIDAD CON AUTHCONTROLLER ⚠️
        [NotMapped]
        public DateTime FechaCreacion
        {
            get => FechaAlta;
            set => FechaAlta = value;
        }

        // Relaciones
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
        public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
        public ICollection<SerieNumeracion> Series { get; set; } = new List<SerieNumeracion>();
    }
}