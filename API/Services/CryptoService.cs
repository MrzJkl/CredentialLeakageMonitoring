using Konscious.Security.Cryptography;
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
            byte[] hash = SHA384.HashData(inputBytes);
            return hash;
        }

        /// <summary>
        /// This method is thread-safe 
        /// </summary>
        public static byte[] HashPassword(string password, byte[] salt)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            var argon2 = new Argon2id(passwordBytes)
            {
                Salt = salt,
                Iterations = 1,            
                MemorySize = 8 * 1024,     
                DegreeOfParallelism = 1  
            };

            return argon2.GetBytes(64);
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
