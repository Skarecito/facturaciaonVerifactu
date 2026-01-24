namespace FacturacionVERIFACTU.API.DTOs
{
    /// <summary>
    /// DTo para crear un nuevo cliente
    /// </summary>
    public class CrearClienteDto
    {
        public string NIF { get; set; } = string.Empty;
        public string Nombre {  get; set; } = string.Empty;
        public string? Direccion {  get; set; } 
        public string? CodigoPostal { get; set; }
        public string? Poblacion {  get; set; }
        public string? Provincia {  get; set; }
        public string? Pais { get; set; } = "España";
        public string? Email {  get; set; }
        public string? Telefono {  get; set; }
        public bool Activo { get; set; }

        // ⭐ CONFIGURACIÓN FISCAL (opcional)
        public bool RegimenRecargoEquivalencia { get; set; } = false;
        public decimal PorcentajeRetencionDefecto { get; set; } = 0;
        public string TipoCliente { get; set; } = "B2B";
        public string? NotasFiscales { get; set; }
    }

    ///<summary>
    ///Dto para actualizar un cliente existente
    /// </summary>
    public class ActualizarClienteDto 
    { 
        public string Nombre { get; set; } = string.Empty;
        public string? Direccion { get; set; } 
        public string? CodigoPostal { get; set; }
        public string? Poblacion { get; set; }
        public string? Provincia { get; set; }
        public string? Pais { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public DateTime FechaModificacion { get; set; }
        public bool Activo { get; set; }
        public bool RegimenRecargoEquivalencia { get; set; }
        public decimal PorcentajeRetencionDefecto { get; set; }
        public string TipoCliente { get; set; } = "B2B";
        public string? NotasFiscales { get; set; }
        
    }

    ///<summary>
    ///DTO de respuesta con datos del cliente
    /// </summary>
    public class ClienteResponseDto
    {
        public int ClienteId { get; set; }
        public string NIF { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public string? CodigoPostal { get; set; }
        public string? Poblacion { get; set; }
        public string? Provincia { get; set; }
        public string? Pais { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public DateTime FechaCreaccion { get; set; }
        public DateTime FechaModificacion { get; set; }
        public bool Activo { get; set; }

        public bool RegimenRecargoEquivalencia { get; set; } = false;
        public decimal PorcentajeRetencionDefecto { get; set; } = 0;
        public string TipoCliente { get; set; } = "B2B";
        public string? NotasFiscales { get; set; }
    }

}
