namespace FacturacionVERIFACTU.Web.Models.DTOs
{
    public class SerieDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = string.Empty;
        public int Ejercicio { get; set; }
        public int ProximoNumero { get; set; }
        public bool Activo { get; set; }
        public bool Bloqueada { get; set; }
        public string Formato { get; set; } = string.Empty;
    }
}
