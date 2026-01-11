using API.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace FacturacionVERIFACTU.API.Data.Entities
{
    [Table("series_facturacion")]
    public class SerieNumeracion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("tenant_id")]
        public int TenantId {  get; set; }

        [Required]
        [MaxLength(10)]
        [Column("codigo")]
        public string Codigo {  get; set; } = string.Empty; //Ej: A B C

        [Required]
        [MaxLength(100)]
        [Column("descripcion")]
        public string Descripcion {  get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [Column("tipo_documento")]
        public string TipoDocumento {  get; set; } = string.Empty; //Presupuesto, Albaran,FActura

        [Column("proximo_numero")]
        public int ProximoNumero { get; set; } = 1;

        [Column("ejercicio")]
        public int Ejercicio {  get; set; }

        [MaxLength(50)]
        [Column("formato")]
        public string Formato { get; set; } = "{SERIRE}-{NUMERO}/{EJERCICIO}";

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Column("bloqueada")]
        public bool Bloqueada { get; set; } = false;

        //Relaciones
        [ForeignKey("TenantId")]
        public Tenant Tenant { get; set; }

        public ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();
        public ICollection<Albaran> Albaranes { get; set; } = new List<Albaran>();
        public ICollection<Factura> Facturas { get; set; } = new List<Factura>();
    }
}
