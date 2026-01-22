using System.Net.Http.Headers;

namespace FacturacionVERIFACTU.Web.Services
{
    public class TokenHandler : DelegatingHandler
    {
        // Propiedad pública que llenaremos desde la página
        public string? Token { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(Token))
            {
                var tokenLimpio = Token.Trim().Trim('"');
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenLimpio);
                Console.WriteLine($"[HANDLER] Token inyectado manualmente: {tokenLimpio.Substring(0, 10)}...");
            }
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
