using FacturacionVERIFACTU.API.Data.Entities;

namespace FacturacionVERIFACTU.API.Data.Entities
{
    /// <summary>
    /// Entidad para almacenar Refresh Tokens JWT
    /// </summary>
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Revoked { get; set; } = false;

        // Navigation property
        public Usuario Usuario { get; set; } = null!;
    }
}
