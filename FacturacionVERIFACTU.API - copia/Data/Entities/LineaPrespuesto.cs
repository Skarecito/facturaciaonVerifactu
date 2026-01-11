using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturacionVERIFACTU.API.Data.Entities
{
    [Table("lineas_presupuesto")]
    public class LineaPresupuesto
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("presupuesto_id")]
        public int PresupuestoId { get; set; }

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

        // CAMPOS ADICIONALES PARA DESCUENTOS
        [Column("porcentaje_descuento", TypeName = "decimal(5,2)")]
        public decimal PorcentajeDescuento { get; set; } = 0;

        [Column("importe_descuento", TypeName = "decimal(18,2)")]
        public decimal ImporteDescuento { get; set; } = 0;

        // BASE IMPONIBLE (después de descuento, antes de IVA)
        [Column("base_imponible", TypeName = "decimal(18,2)")]
        public decimal BaseImponible { get; set; } = 0;

        // IVA (ya lo tenías)
        [Column("iva", TypeName = "decimal(5,2)")]
        public decimal IVA { get; set; }

        [Column("recargo_equivalencia", TypeName = "decimal(5,2)")]
        public decimal RecargoEquivalencia { get; set; } = 0;

        // IMPORTE IVA (calculado)
        [Column("importe_iva", TypeName = "decimal(18,2)")]
        public decimal ImporteIva { get; set; } = 0;

        // IMPORTE TOTAL (ya lo tenías - base + IVA)
        [Column("importe", TypeName = "decimal(18,2)")]
        public decimal Importe { get; set; }

        // Propiedades calculadas
        [NotMapped]
        public decimal CuotaIVA => Math.Round(Importe * IVA / 100, 2);

        [NotMapped]
        public decimal CuotaRecargo => Math.Round(Importe * RecargoEquivalencia / 100, 2);

        [NotMapped]
        public decimal TotalLinea => Importe + CuotaIVA + CuotaRecargo;

        //Relaciones
        [ForeignKey("PresupuestoId")]
        public Presupuesto Presupuesto { get; set; } = null;

        [ForeignKey("ProductoId")]
        public Producto Producto { get; set; } 


    }
}
