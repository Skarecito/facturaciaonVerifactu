using System.ComponentModel.DataAnnotations;

namespace FacturacionVERIFACTU.API.DTOs
{
    public class CreateUsuarioDto
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Email invalido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage ="La contraseña es obligatoria")]
        [MinLength(8,ErrorMessage ="Minimo 8 caracteres")]
        public string Password {  get; set; } = string.Empty;

        [Required(ErrorMessage ="El nombre es obligatorio")]
        public string NombreCompleto {  get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es obligatorio")]
        [RegularExpression("^(Admin|Usuario|Consulta)$", ErrorMessage = "Rol inválido")]
        public string Role { get; set; } = "Usuario";

        public bool Activo { get; set; } = true;
    }

    public class UpdateUsuarioDto
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es obligatorio")]
        [RegularExpression("^(Admin|Usuario|Consulta)$")]
        public string Role { get; set; } = "Usuario";

        public bool Activo { get; set; } = true;

    }

    public class UsuarioResponseDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string NombreCompleto { get; set; }
        public string Role { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int TenantId { get; set; }
        public string NombreEmpresa {  get; set; }
    }

    public class CambiarEstadoUsuarioDto
    {
        [Required]
        public bool Activo { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "La contraseña actual es obligatoria")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "La contraseña debe contener mayúsculas, minúsculas, números y caracteres especiales")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debes confirmar la contraseña")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ResetPasswordRespondeDto
    {
        public string Email { get; set; } = string.Empty;
        public string PasswordTemporal { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
    }
}
