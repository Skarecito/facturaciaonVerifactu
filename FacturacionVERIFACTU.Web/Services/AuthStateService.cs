using FacturacionVERIFACTU.Web.Models;

namespace FacturacionVERIFACTU.Web.Services;

public class AuthStateService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string TOKEN_SESSION_KEY = "AuthToken";
    private const string USER_SESSION_KEY = "UserData";

    public AuthStateService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? Token
    {
        get
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            return session?.GetString(TOKEN_SESSION_KEY);
        }
    }

    public UserInfo? User
    {
        get
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            var userData = session?.GetString(USER_SESSION_KEY);

            if (string.IsNullOrEmpty(userData))
                return null;

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<UserInfo>(userData);
            }
            catch
            {
                return null;
            }
        }
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    public void SetAuthentication(string token, UserInfo user)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session != null)
        {
            session.SetString(TOKEN_SESSION_KEY, token);
            session.SetString(USER_SESSION_KEY, System.Text.Json.JsonSerializer.Serialize(user));
        }
    }

    public void ClearAuthentication()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        session?.Remove(TOKEN_SESSION_KEY);
        session?.Remove(USER_SESSION_KEY);
    }
}