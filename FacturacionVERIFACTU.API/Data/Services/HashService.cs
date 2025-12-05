using BCrypt.Net;

namespace FacturacionVERIFACTU.API.Data.Services
{
    /// <summary>
    /// Servicio para hash de contraseñas con BCrypt
    /// </summary>
    public interface IHashService
    {
        string Hash(string password);
        bool Verify(string password, string hash);
    }

    public class HashService : IHashService
    {
        /// <summary>
        /// Genera hash de contraseña con BCrypt (work factor 12)
        /// </summary>
        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        /// <summary>
        /// Verifica contraseña contra hash
        /// </summary>
        public bool Verify(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}