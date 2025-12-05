using System.ComponentModel.DataAnnotations;

namespace FacturacionVERIFACTU.API.DTOs
{

    // ===== Request DTOs
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8,ErrorMessage ="La contraseña debe contener al menos 8 caracteres")]
        public string Password { get; set; } = string.Empty ;

        [Required]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        public string NombreEmpresa {  get; set; } = string.Empty ;

        [Required]
        [RegularExpression(@"^[A-Z]\d{8}$", ErrorMessage = "NIF inválido (formato: A12345678)")]
        public string NIF { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken {  get; set; } = string.Empty;
    }

    // ===== RESPONSE DTOs ====

    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; }= string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserInfo User { get; set; } = new();
    }

    public class UserInfo
    {
        public int UserId {  get; set; }
        public string Email { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public int TenantId {  get; set; }
        public string NombreEmpresa { get; set; } = string.Empty;
        public string Role {  get; set; } = string.Empty;
    }
}
