using System.ComponentModel.DataAnnotations;

namespace FacturacionVERIFACTU.Web.Models.DTOs
{
    /// <summary>
    /// Modelo de registro de nueva empresa + usuario administrador
    /// </summary>
    public class RegisterRequest
    {
        // ─── PASO 1: Datos de la empresa ───────────────────────────────

        [Required(ErrorMessage = "El nombre de la empresa es obligatorio")]
        [MinLength(3, ErrorMessage = "Mínimo 3 caracteres")]
        [MaxLength(100, ErrorMessage = "Máximo 100 caracteres")]
        public string NombreEmpresa { get; set; } = string.Empty;

        [Required(ErrorMessage = "El NIF/CIF es obligatorio")]
        [RegularExpression(@"^[A-Z]\d{8}$|^\d{8}[A-Z]$",
            ErrorMessage = "NIF inválido (formatos: B12345678 ó 12345678A)")]
        public string NIF { get; set; } = string.Empty;

        // ─── PASO 2: Datos del administrador ───────────────────────────

        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [MinLength(3, ErrorMessage = "Mínimo 3 caracteres")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(8, ErrorMessage = "Mínimo 8 caracteres")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Debe contener mayúsculas, minúsculas, números y un carácter especial (@$!%*?&)")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirma la contraseña")]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }
}
