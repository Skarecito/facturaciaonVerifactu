namespace FacturacionVERIFACTU.API.Data.Interfaces
{
    public interface IHashService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);

        // Métodos legacy por compatibilidad
        string Hash(string password);
        bool Verify(string password, string hash);
    }
}
