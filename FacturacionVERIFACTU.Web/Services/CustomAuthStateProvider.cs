using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FacturacionVERIFACTU.Web.Models;

namespace FacturacionVERIFACTU.Web.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly AuthStateService _authState;
    private readonly HttpClient _httpClient;

    public CustomAuthStateProvider(
        AuthStateService authState,
        HttpClient httpClient)
    {
        _authState = authState;
        _httpClient = httpClient;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = _authState.Token;

        if (string.IsNullOrEmpty(token))
        {
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }

        try
        {
            var claims = ParseClaimsFromJwt(token);

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            return Task.FromResult(new AuthenticationState(user));
        }
        catch
        {
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }
    }

    public void NotifyUserAuthentication(string token, UserInfo user)
    {
        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var principal = new ClaimsPrincipal(identity);

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    public void NotifyUserLogout()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;

        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymousUser)));
    }

    private List<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);

            claims.AddRange(token.Claims);
        }
        catch { }

        return claims;
    }
}