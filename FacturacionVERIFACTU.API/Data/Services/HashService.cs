using FacturacionVERIFACTU.API.Data.Interfaces;

namespace FacturacionVERIFACTU.API.Data.Services
{
    public class HashService : IHashService
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, 12);
        }

        public bool VerifyPassword(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                return false;
            }
        }

        // Métodos legacy por compatibilidad
        public string Hash(string password)
        {
            return HashPassword(password);
        }

        public bool Verify(string password, string hash)
        {
            return VerifyPassword(password, hash);
        }
    }
}