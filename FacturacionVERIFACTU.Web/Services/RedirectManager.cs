using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace FacturacionVERIFACTU.Web.Services;

public sealed class RedirectManager
{
    private readonly NavigationManager _navigationManager;

    public RedirectManager(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    [DoesNotReturn]
    public void RedirectTo(string uri)
    {
        uri ??= "";

        if (!Uri.IsWellFormedUriString(uri, UriKind.Relative))
        {
            uri = _navigationManager.ToBaseRelativePath(uri);
        }

        _navigationManager.NavigateTo(uri);
        throw new InvalidOperationException($"{nameof(RedirectManager)} can only be used during static rendering.");
    }
}