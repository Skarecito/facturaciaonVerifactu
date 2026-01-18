using FacturacionVERIFACTU.Web.Models.DTOs;

namespace FacturacionVERIFACTU.Web.Services

{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task LogoutAsync();
        Task<string> GetTokenAsync();
        Task<UserInfo> GetCurrentUserAsync();
        Task<bool> IsAuthenticatedAsync();
    }
}
