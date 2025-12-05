using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturacionVERIFACTU.API.Models
{
    [Table("lineas_factura")]
    public class LineaFactura
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("factura_id")]
        public Guid FacturaId { get; set; }

        [Required]
        [Column("orden")]
        public int Orden { get; set; }

        [Required]
        [Column("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        [Column("cantidad", TypeName = "decimal(10,3)")]
        public decimal Cantidad { get; set; }

        [Required]
        [Column("precio_unitario", TypeName = "decimal(12,2)")]
        public decimal PrecioUnitario { get; set; }

        [Required]
        [Column("tipo_iva", TypeName = "decimal(5,2)")]
        public decimal TipoIva { get; set; }

        [Required]
        [Column("importe", TypeName = "decimal(12,2)")]
        public decimal Importe { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navegación
        [ForeignKey("FacturaId")]
        public Factura Factura { get; set; } = null!;
    }
}