using System.ComponentModel.DataAnnotations;

namespace FacturacionVERIFACTU.Web.Models.DTOs
{
    public class TenantResponseDto
    {
        public int Id { get; set; }
        public string RazonSocial { get; set; } = string.Empty;
        public string NIF { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public string? CodigoPostal { get; set; }
        public string? Poblacion { get; set; }
        public string? Provincia { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public bool TieneLogo { get; set; }
        public string? RegistroMercantil { get; set; }
        public string? Tomo { get; set; }
        public string? Libro { get; set; }
        public string? Folio { get; set; }
        public string? Seccion { get; set; }
        public string? Hoja { get; set; }
        public string? Inscripcion { get; set; }
    }

    public class TenantUpdateDto
    {
        [Required(ErrorMessage = "La razón social es obligatoria")]
        [MaxLength(100)]
        public string RazonSocial { get; set; } = string.Empty;

        [Required(ErrorMessage = "El NIF es obligatorio")]
        [MaxLength(20)]
        public string NIF { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Direccion { get; set; }

        [MaxLength(10)]
        public string? CodigoPostal { get; set; }

        [MaxLength(100)]
        public string? Poblacion { get; set; }

        [MaxLength(100)]
        public string? Provincia { get; set; }

        [MaxLength(20)]
        public string? Telefono { get; set; }

        [MaxLength(100)]
        [EmailAddress(ErrorMessage = "Email no válido")]
        public string? Email { get; set; }

        [MaxLength(200)]
        public string? RegistroMercantil { get; set; }

        [MaxLength(50)]
        public string? Tomo { get; set; }

        [MaxLength(50)]
        public string? Libro { get; set; }

        [MaxLength(50)]
        public string? Folio { get; set; }

        [MaxLength(50)]
        public string? Seccion { get; set; }

        [MaxLength(50)]
        public string? Hoja { get; set; }

        [MaxLength(50)]
        public string? Inscripcion { get; set; }
    }
}

