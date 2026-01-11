namespace FacturacionVERIFACTU.API.Data.Interfaces
{
        /// <summary>
        /// Contexto de Tenant para Multi-tenancy
        /// </summary>
        public interface ITenantContext
        {
            int? GetTenantId();
            void SetTenantId(int tenantId);
            string? GetTenantSchema();
        }
    }

