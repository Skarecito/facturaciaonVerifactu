using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Json;
using FacturacionVERIFACTU.Web.Models.DTOs;
using FacturacionVERIFACTU.Web.Services;

namespace FacturacionVERIFACTU.Web.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly ILogger<AuthService> _logger;

    private const string TOKEN_KEY = "authToken";
    private const string USER_KEY = "currentUser";

    public AuthService(
        HttpClient httpClient,
        ProtectedSessionStorage sessionStorage,
        ILogger<AuthService> logger)
    {
        _httpClient = httpClient;
        _sessionStorage = sessionStorage;
        _logger = logger;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)  // ← CORREGIDO con ?
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/login", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Login fallido: {Error}", error);
                return null;  // ← Retorna null si falla
            }

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (loginResponse == null)
            {
                _logger.LogError("Respuesta de login nula");
                return null;  // ← Retorna null si deserialización falla
            }

            // Guardar en SessionStorage (se borra al cerrar navegador)
            await _sessionStorage.SetAsync(TOKEN_KEY, loginResponse.AccessToken);
            await _sessionStorage.SetAsync(USER_KEY, loginResponse.User);

            _logger.LogInformation("Login exitoso para {Email}", request.Email);
            return loginResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en LoginAsync");
            return null;  // ← Retorna null si hay excepción
        }
    }

    public async Task<LoginResponse?> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/register", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Registro fallido: {Error}", error);
                return null;
            }

            var registerResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (registerResponse == null)
            {
                _logger.LogError("Respuesta de registro nula");
                return null;
            }

            // Guardar tokens en SessionStorage (igual que en login)
            await _sessionStorage.SetAsync(TOKEN_KEY, registerResponse.AccessToken);
            await _sessionStorage.SetAsync(USER_KEY, registerResponse.User);

            _logger.LogInformation("Registro exitoso para {Email}", request.Email);
            return registerResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en RegisterAsync");
            return null;
        }
    }


    public async Task LogoutAsync()
    {
        try
        {
            await _sessionStorage.DeleteAsync(TOKEN_KEY);
            await _sessionStorage.DeleteAsync(USER_KEY);
            _logger.LogInformation("Logout exitoso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en LogoutAsync");
        }
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            var result = await _sessionStorage.GetAsync<string>(TOKEN_KEY);
            return result.Success ? result.Value : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<UserInfo?> GetCurrentUserAsync()
    {
        try
        {
            var result = await _sessionStorage.GetAsync<UserInfo>(USER_KEY);
            return result.Success ? result.Value : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }
}