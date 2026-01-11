using FacturacionVERIFACTU.API.Data.Interfaces;

namespace FacturacionVERIFACTU.API.Data.Services
{
    /// <summary>
    /// Implementación de ITenantContext - Obtiene TenantId y Schema del token JWT
    /// </summary>
    public class TenantContext : ITenantContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private int? _tenantId;

        public TenantContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Obtiene el TenantId del claim del token JWT
        /// </summary>
        public int? GetTenantId()
        {
            // Buscar el claim "TenantId" o "tenant_id" en el token
            var tenantIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirst("TenantId")?.Value
                ?? _httpContextAccessor.HttpContext?.User.FindFirst("tenant_id")?.Value;

            if (int.TryParse(tenantIdClaim, out int tenantId))
            {
                return tenantId;
            }

            return null;
        }

        /// <summary>
        /// Obtiene el Schema del tenant desde el claim del token JWT
        /// </summary>
        public string? GetTenantSchema()
        {
            // Obtener el schema del claim "TenantSchema"
            var schema = _httpContextAccessor.HttpContext?.User
                .FindFirst("TenantSchema")?.Value;

            return schema;
        }

        public void SetTenantId(int tenantId)
        {
            _tenantId = tenantId;
        }
}
}