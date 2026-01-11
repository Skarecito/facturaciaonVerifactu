using FacturacionVERIFACTU.Web.Models;
using System.Net.Http.Json;

namespace FacturacionVERIFACTU.Web.Services;

public class AuthenticationService
{
    private readonly AuthStateService _authState;
    private readonly CustomAuthStateProvider _authStateProvider;
    private const string API_BASE_URL = "http://localhost:5121";

    public AuthenticationService(
        AuthStateService authState,
        CustomAuthStateProvider authStateProvider)
    {
        _authState = authState;
        _authStateProvider = authStateProvider;
    }

    public async Task<LoginResponse> LoginAsync(string email, string password)
    {
        using var httpClient = new HttpClient
        {
            BaseAddress = new Uri(API_BASE_URL),
            Timeout = TimeSpan.FromSeconds(30)
        };

        try
        {
            var loginRequest = new LoginRequest
            {
                Email = email,
                Password = password
            };

            var response = await httpClient.PostAsJsonAsync("/api/Auth/login", loginRequest);

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiAuthResponse>();

                if (apiResponse?.AccessToken != null)
                {
                    var userInfo = new UserInfo
                    {
                        Id = apiResponse.User.UserId,
                        Email = apiResponse.User.Email,
                        NombreCompleto = apiResponse.User.NombreCompleto,
                        TenantId = apiResponse.User.TenantId.ToString(),
                        Role = apiResponse.User.Role
                    };

                    // Guardar en memoria
                    _authState.SetAuthentication(apiResponse.AccessToken, userInfo);

                    // Notificar cambio
                    _authStateProvider.NotifyUserAuthentication(apiResponse.AccessToken, userInfo);

                    return new LoginResponse
                    {
                        Success = true,
                        Token = apiResponse.AccessToken,
                        User = userInfo
                    };
                }
            }

            return new LoginResponse
            {
                Success = false,
                Message = $"Error: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new LoginResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    public void Logout()
    {
        _authState.ClearAuthentication();
        _authStateProvider.NotifyUserLogout();
    }

    public UserInfo? GetUser() => _authState.User;
    public bool IsAuthenticated() => _authState.IsAuthenticated;

    private class ApiAuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public ApiUserInfo User { get; set; } = new();
    }

    private class ApiUserInfo
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public int TenantId { get; set; }
        public string NombreEmpresa { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}