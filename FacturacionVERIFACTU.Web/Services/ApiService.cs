using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Json;

namespace FacturacionVERIFACTU.Web.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly ProtectedSessionStorage _sessionStorage;
    private const string TOKEN_KEY = "authToken";

    public ApiService(
        HttpClient httpClient,
        ProtectedSessionStorage sessionStorage)
    {
        _httpClient = httpClient;
        _sessionStorage = sessionStorage;
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        await EnsureAuthenticatedAsync();

        try
        {
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en GET {endpoint}: {ex.Message}");
            throw;
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        await EnsureAuthenticatedAsync();

        try
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, data);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en POST {endpoint}: {ex.Message}");
            throw;
        }
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        await EnsureAuthenticatedAsync();

        try
        {
            var response = await _httpClient.PutAsJsonAsync(endpoint, data);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en PUT {endpoint}: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        await EnsureAuthenticatedAsync();

        try
        {
            var response = await _httpClient.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en DELETE {endpoint}: {ex.Message}");
            throw;
        }
    }

    private async Task EnsureAuthenticatedAsync()
    {
        try
        {
            var tokenResult = await _sessionStorage.GetAsync<string>(TOKEN_KEY);
            if (tokenResult.Success && !string.IsNullOrEmpty(tokenResult.Value))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResult.Value);
            }
        }
        catch
        {
            // Si falla, continuar sin token
        }
    }
}