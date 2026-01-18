using Microsoft.AspNetCore.Components.Authorization;
using FacturacionVERIFACTU.Web.Components;
using FacturacionVERIFACTU.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// SERVICIOS DE RAZOR COMPONENTS
// ========================================
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ========================================
// AUTENTICACIÓN Y AUTORIZACIÓN
// ========================================
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ========================================
// HTTP CLIENT PARA API
// ========================================
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001";
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});

builder.Services.AddScoped<IApiService, ApiService>();

var app = builder.Build();

// ========================================
// PIPELINE HTTP
// ========================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();