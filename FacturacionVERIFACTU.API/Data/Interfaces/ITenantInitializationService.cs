namespace FacturacionVERIFACTU.API.Data.Interfaces
{
    public interface ITenantInitializationService
    {
        Task IncicializarTenantAsync(int tenantId);
    }
}
