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

        [Column("iva", TypeName = "decimal(5,2)")]
        public decimal IVA { get; set; }

        [Column("recargo_equivalencia", TypeName = "decimal(5,2)")]
        public decimal RecargoEquivalencia { get; set; } = 0;

        [Column("iva_percent_snapshot", TypeName = "decimal(5,2)")]
        public decimal IvaPercentSnapshot { get; set; }

        [Column("re_percent_snapshot", TypeName = "decimal(5,2)")]
        public decimal RePercentSnapshot { get; set; }

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

        [Column("importe_recargo", TypeName = "decimal(18,2)")]
        public decimal ImporteRecargo { get; set; } = 0;

        [Column("total_linea", TypeName = "decimal(18,2)")]
        public decimal TotalLineaSnapshot { get; set; }

        

        // Propiedades calculadas
        [NotMapped]
        public decimal CuotaIVA => Math.Round(BaseImponible * IvaPercentSnapshot / 100, 2);

        [NotMapped]
        public decimal CuotaRecargo => Math.Round(BaseImponible * RePercentSnapshot / 100, 2);

        [NotMapped]
        public decimal TotalLinea => BaseImponible + CuotaIVA + CuotaRecargo;

        // Relaciones
        [ForeignKey("AlbaranId")]
        public Albaran Albaran { get; set; } = null!;

        [ForeignKey("ProductoId")]
        public Producto? Producto { get; set; }

        [ForeignKey("TipoImpuestoId")]
        public TipoImpuesto? TipoImpuesto { get; set; }
    }
}
