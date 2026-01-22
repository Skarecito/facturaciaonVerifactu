using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FacturacionVERIFACTU.Web.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IAuthService _authService;
        private readonly ILogger<CustomAuthStateProvider> _logger;

        public CustomAuthStateProvider(
            IAuthService authService,
            ILogger<CustomAuthStateProvider> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _authService.GetTokenAsync();

                // 1. Si NO hay token, devolver estado Anónimo inmediatamente
                if (string.IsNullOrEmpty(token))
                {
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                // 2. Si SÍ hay token, procedemos a leerlo
                var handler = new JwtSecurityTokenHandler();

                // Validamos que sea un JWT bien formado
                if (!handler.CanReadToken(token))
                {
                    await _authService.LogoutAsync();
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                var jwtToken = handler.ReadJwtToken(token);

                // 3. Verificar expiración
                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    _logger.LogWarning("El token ha expirado.");
                    await _authService.LogoutAsync();
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                // 4. Crear la identidad con los Claims del token
                // IMPORTANTE: El string "jwt" indica que el usuario ESTÁ autenticado
                var claims = jwtToken.Claims.ToList();
                var identity = new ClaimsIdentity(claims, "jwt");
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo estado de autenticación");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public void NotifyAuthenticationStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
