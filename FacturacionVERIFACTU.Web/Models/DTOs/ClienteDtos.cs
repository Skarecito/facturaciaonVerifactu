using System.ComponentModel.DataAnnotations;

namespace FacturacionVERIFACTU.Web.Models.DTOs;

public class ClienteDto
{
    public int ClienteId { get; set; }

    [Required(ErrorMessage = "El NIF es obligatorio.")]
    [StringLength(20, ErrorMessage = "El NIF no puede superar 20 caracteres.")]
    public string NIF { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(200, ErrorMessage = "El nombre no puede superar 200 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "El email no es válido.")]
    public string? Email { get; set; }

    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Poblacion { get; set; }
    public string? Provincia { get; set; }
    public string? Pais { get; set; } = "España";
    public bool Activo { get; set; }
    public bool RegimenRecargoEquivalencia {  get; set; }
    public decimal PorcentajeRetencionDefecto {  get; set; }
    public string TipoCliente { get; set; } = "B2B";
    public string? NotasFiscales {  get; set; }
}

public class CrearClienteDto
{
    [Required(ErrorMessage = "El NIF es obligatorio.")]
    [StringLength(20, ErrorMessage = "El NIF no puede superar 20 caracteres.")]
    public string NIF { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(200, ErrorMessage = "El nombre no puede superar 200 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "El email no es válido.")]
    public string? Email { get; set; }

    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Poblacion { get; set; }
    public string? Provincia { get; set; }
    public string? Pais { get; set; } = "España";
    public bool Activo { get; set; }
    public bool RegimenRecargoEquivalencia {  get; set; }
    public decimal PorcentajeRetencionDefecto {  get; set; }
    public string TipoCliente { get; set; } = "B2B";
    public string? NotasFiscales { get; set; }
}

public class ActualizarClienteDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(200, ErrorMessage = "El nombre no puede superar 200 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "El email no es válido.")]
    public string? Email { get; set; }

    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Poblacion { get; set; }
    public string? Provincia { get; set; }
    public string? Pais { get; set; } = "España";
    public bool Activo { get;set; }
    public bool RegimenRecargoEquivalencia { get; set; }
    public decimal PorcentajeRetencionDefecto { get; set; }
    public string TipoCliente { get; set; } = "B2B";
    public string? NotasFiscales { get; set; }
}
