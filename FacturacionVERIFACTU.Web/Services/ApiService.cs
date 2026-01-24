using Microsoft.AspNetCore.Identity.Data;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace FacturacionVERIFACTU.Web.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthService _authService;
        private readonly ILogger<ApiService> _logger;

        public ApiService(
            HttpClient httpCient,
            IAuthService authService,
            ILogger<ApiService> logger)
        {
            _httpClient = httpCient;
            _authService = authService;
            _logger = logger;
        }

        private async Task AddAuthHeaderAsync()
        {
            var token = await _authService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                await AddAuthHeaderAsync();
                var response = await _httpClient.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("GET {Endpoint} falló: {Status}", endpoint, response.StatusCode);
                    return default;
                }

                return await response.Content.ReadFromJsonAsync<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GET {Endpoint}", endpoint);
                return default;
            }
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                await AddAuthHeaderAsync();
                var response = await _httpClient.PostAsJsonAsync(endpoint, data);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("POST {Endpoint} falló: {Status}", endpoint, response.StatusCode);
                    return default;
                }

                return await response.Content.ReadFromJsonAsync<TResponse>();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error en POST {Endpoint}", endpoint);
                return default;
            }
        }

        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                await AddAuthHeaderAsync();
                var response = await _httpClient.PutAsJsonAsync(endpoint, data);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("PUT {Endpoint} falló: {Status}", endpoint, response.StatusCode);
                    return default;
                }

                return await response.Content.ReadFromJsonAsync<TResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en PUT {Endpoint}", endpoint);
                return default;
            }
        }

        public async Task<TResponse?> PatchAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                await AddAuthHeaderAsync();
                var request = new HttpRequestMessage(HttpMethod.Patch, endpoint)
                {
                    Content = JsonContent.Create(data)
                };
                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("PATC {Endpoint} fallo: {Status}", endpoint, response.StatusCode);
                    return default;
                }
                return await response.Content.ReadFromJsonAsync<TResponse>();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error en PATCH {Endpoint}", endpoint);
                return default;
            }
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                await AddAuthHeaderAsync();
                var response = await _httpClient.DeleteAsync(endpoint);
                return response.IsSuccessStatusCode;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error en DELETE {Endpoint}", endpoint);
                return false;
            }
        }
    }
}
