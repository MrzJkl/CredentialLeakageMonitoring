using Konscious.Security.Cryptography;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace CredentialLeakageMonitoring.API.Services
{
    public class CryptoService()
    {
        private const int SaltLength = 16;
        public static readonly string AlgorithmForPassword = nameof(SHA512);
        public static readonly string AlgorithmForEmail = nameof(SHA512);
        // Argon2 Version 1.3 (v=19) regarding:
        // https://www.nuget.org/packages/Konscious.Security.Cryptography.Argon2
        public static readonly string AlgorithmVersionForPassword = nameof(SHA512);

        public byte[] HashEmail(string email)
        {
            email = email.Trim().ToLowerInvariant();
            byte[] inputBytes = Encoding.UTF8.GetBytes(email);
            byte[] hash = SHA512.HashData(inputBytes);
            return hash;
        }

        public byte[] HashPassword(string password, byte[] salt)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] saltedPassword = new byte[salt.Length + passwordBytes.Length];

            // Combine PasswordSalt + Passwort (PasswordSalt first)
            Buffer.BlockCopy(salt, 0, saltedPassword, 0, salt.Length);
            Buffer.BlockCopy(passwordBytes, 0, saltedPassword, salt.Length, passwordBytes.Length);

            byte[] hash = SHA512.HashData(saltedPassword);

            return hash;
        }
        
        public byte[] GenerateRandomSalt()
        {
            byte[] salt = new byte[SaltLength];
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }
    }
}
