using FacturacionVERIFACTU.API.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Data.Entities
{
    [Table("lineas_factura")]
    public class LineaFactura
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("factura_id")]
        public int FacturaId { get; set; }

        [Column("producto_id")]
        public int? ProductoId { get; set; }

        [Column("tipo_impuesto_id")]
        public int? TipoImpuestoId { get; set; }

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

        [Column("iva_percent_snapshot", TypeName = "decimal(5,2)")]
        public decimal IvaPercentSnapshot { get; set; }

        [Column("re_percent_snapshot", TypeName = "decimal(5,2)")]
        public decimal RePercentSnapshot { get; set; }

        [Column("base_imponible", TypeName = "decimal(18,2)")]
        public decimal BaseImponible { get; set; }

        [Column("importe_iva", TypeName = "decimal(18,2)")]
        public decimal ImporteIva { get; set; }

        [Column("importe_recargo", TypeName = "decimal(18,2)")]
        public decimal ImporteRecargo { get; set; }

        [Column("importe", TypeName = "decimal(18,2)")]
        public decimal Importe { get; set; }

        // Propiedades calculadas (no se guardan en BD)
        [NotMapped]
        public decimal CuotaIVA => Math.Round(BaseImponible * IvaPercentSnapshot / 100, 2);

        [NotMapped]
        public decimal CuotaRecargo => Math.Round(BaseImponible * RePercentSnapshot / 100, 2);

        [NotMapped]
        public decimal TotalLinea => BaseImponible + CuotaIVA + CuotaRecargo;

        // Relaciones
        [ForeignKey("FacturaId")]
        public Factura Factura { get; set; } = null!;

        [ForeignKey("ProductoId")]
        public Producto? Producto { get; set; }

        [ForeignKey("TipoImpuestoId")]
        public TipoImpuesto? TipoImpuesto { get; set; }
    }
}
