using FacturacionVERIFACTU.API.Data.Interfaces;



namespace FacturacionVERIFACTU.API.Data.Services
{
    /// <summary>
    /// Implementación de ITenantContext - Almacena el TenantId del usuario autenticado
    /// </summary>
    public class TenantContext : ITenantContext
    {
        private int _tenantId;

        public int TenantId => _tenantId;

        public void SetTenantId(int tenantId)
        {
            _tenantId = tenantId;
        }

        public int? GetTenantId() 
        { 
            return _tenantId;
        }
    }
}