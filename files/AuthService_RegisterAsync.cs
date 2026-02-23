// ─────────────────────────────────────────────────────────────────────────────
// AÑADIR este método a AuthService.cs, justo debajo de LoginAsync
// ─────────────────────────────────────────────────────────────────────────────

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
