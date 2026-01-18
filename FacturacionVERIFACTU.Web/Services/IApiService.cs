using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FacturacionVERIFACTU.Web.Services;

public interface IApiService
{
    Task<T?> GetAsync<T>(string endpoint);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data);
    Task<bool> DeleteAsync(string endpoint);
}