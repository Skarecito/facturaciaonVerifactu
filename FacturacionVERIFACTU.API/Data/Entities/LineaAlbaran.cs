using FacturacionVERIFACTU.API.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Data.Entities
{
    [Table("lineas_albaran")]
    public class LineaAlbaran
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("albaran_id")]
        public int AlbaranId { get; set; }

        [Column("producto_id")]
        public int? ProductoId { get; set; }

        [Column("orden")]
        public int Orden { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [Column("cantidad", TypeName = "decimal(18,2)")]
        public decimal Cantidad { get; set; }

        [Column("precio_unitario", TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [Column("iva", TypeName = "decimal(5,2)")]
        public decimal IVA { get; set; }

        [Column("importe", TypeName = "decimal(18,2)")]
        public decimal Importe { get; set; }

        [Column("porcentaje_descuento", TypeName = "decimal(5,2)")]
        public decimal PorcentajeDescuento { get; set; } = 0;

        [Column("importe_descuento", TypeName = "decimal(18,2)")]
        public decimal ImporteDescuento { get; set; } = 0;

        [Column("base_imponible", TypeName = "decimal(18,2)")]
        public decimal BaseImponible { get; set; } = 0;

        [Column("importe_iva", TypeName = "decimal(18,2)")]
        public decimal ImporteIva { get; set; } = 0;

        // Relaciones
        [ForeignKey("AlbaranId")]
        public Albaran Albaran { get; set; } = null!;

        [ForeignKey("ProductoId")]
        public Producto? Producto { get; set; }
    }
}
