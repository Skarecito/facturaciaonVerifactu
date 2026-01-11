using Microsoft.AspNetCore.Components;

namespace FacturacionVERIFACTU.Web.Services;

public sealed class IdentityRedirectManager
{
    private readonly NavigationManager _navigationManager;

    public IdentityRedirectManager(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public void RedirectTo(string? uri)
    {
        uri ??= "";

        if (!Uri.IsWellFormedUriString(uri, UriKind.Relative))
        {
            uri = _navigationManager.ToBaseRelativePath(uri);
        }

        // Simple navegación - las cookies persisten
        _navigationManager.NavigateTo(uri, forceLoad: false);
    }
}