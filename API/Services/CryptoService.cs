using System.Security.Cryptography;
using System.Text;

namespace CredentialLeakageMonitoring.API.Services
{
    /// <summary>
    /// This class is thread-safe because it does not use shared (mutable) fields or instance variables.
    /// </summary>
    public static class CryptoService
    {
        private const int SaltLength = 16;

        /// <summary>
        /// Thread safe
        /// </summary>
        public static byte[] HashEmail(string email)
        {
            email = email.Trim().ToLowerInvariant();
            byte[] inputBytes = Encoding.UTF8.GetBytes(email);
            byte[] hash = SHA3_512.HashData(inputBytes);
            return hash;
        }

        /// <summary>
        /// This method is thread-safe 
        /// </summary>
        public static byte[] HashPassword(string password, byte[] salt)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] saltedPassword = new byte[salt.Length + passwordBytes.Length];

            // Combine Salt + Passwort (salt first)
            Buffer.BlockCopy(salt, 0, saltedPassword, 0, salt.Length);
            Buffer.BlockCopy(passwordBytes, 0, saltedPassword, salt.Length, passwordBytes.Length);

            byte[] hash = SHA3_512.HashData(saltedPassword);

            return hash;
        }

        /// <summary>
        /// Thread safe
        /// </summary>
        public static byte[] GenerateRandomSalt()
        {
            byte[] salt = new byte[SaltLength];
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }
    }
}
