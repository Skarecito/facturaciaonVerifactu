using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturacionVERIFACTU.API.Models
{
    [Table("empresas")]
    public class Empresa
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("nombre")]
        [MaxLength(255)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [Column("nif")]
        [MaxLength(20)]
        public string Nif { get; set; } = string.Empty;

        [Column("direccion")]
        public string? Direccion { get; set; }

        [Column("codigo_postal")]
        [MaxLength(10)]
        public string? CodigoPostal { get; set; }

        [Column("municipio")]
        [MaxLength(100)]
        public string? Municipio { get; set; }

        [Column("provincia")]
        [MaxLength(100)]
        public string? Provincia { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navegación
        public ICollection<Factura> Facturas { get; set; } = new List<Factura>();
    }
}