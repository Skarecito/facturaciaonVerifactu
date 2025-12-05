using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturacionVERIFACTU.API.Data.Entities
{
    [Table("lineas_presupuesto")]
    public class LineaPrespuesto
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("presupuesto_id")]
        public int PresupuestoId {  get; set; }

        [Column("producto_id")]
        public int? ProductoId {  get; set; }

        [Column("orden")]
        public int Orden {  get; set; }

        [Required]
        [MaxLength(200)]
        [Column("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [Column("cantidad", TypeName ="decimal(18,2)")]
        public decimal Cantidad {  get; set; }

        [Column("precio_unitario",TypeName ="decimal(18,2)")]
        public decimal PrecioUnitario {  get; set; }

        [Column("iva",TypeName ="decimal(5,2)")]
        public decimal IVA { get; set; }

        [Column("importe", TypeName ="decimal(18,2)")]
        public decimal Importe {  get; set; }

        //Relaciones
        [ForeignKey("PresupuestoId")]
        public Presupuesto Presupuesto { get; set; } = null;

        [ForeignKey("ProductoId")]
        public Producto Producto { get; set; } 


    }
}
