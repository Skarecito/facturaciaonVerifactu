namespace FacturacionVERIFACTU.API.Models.VERIFACTU
{
    public class ResultadoEnvio
    {
        public bool Exitoso {  get; set; }
        public string? CodigoRespuesta {  get; set; }
        public string? Mensaje {  get; set; }
        public string? CSV {  get; set; } //Codigo seguro de verificacion
        public DateTime? FechaRegistro {  get; set; }
        public List<string> Errores { get; set; } = new();
        public string? XmlEnviado {  get; set; }
        public string? XmlRespuesta { get; set;  }
    }
}
