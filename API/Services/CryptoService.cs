using Konscious.Security.Cryptography;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace CredentialLeakageMonitoring.API.Services
{
    public class CryptoService(ILogger<CryptoService> log)
    {
        private const int SaltLength = 16;
        public static readonly string AlgorithmForPassword = nameof(Argon2id);
        public static readonly string AlgorithmForEmail = nameof(SHA3_512);
        // Argon2 Version 1.3 (v=19) regarding:
        // https://www.nuget.org/packages/Konscious.Security.Cryptography.Argon2
        public static readonly string AlgorithmVersionForPassword = "v=19";

        public byte[] HashEmail(string email)
        {
            Stopwatch sw = Stopwatch.StartNew();
            email = email.Trim().ToLowerInvariant();
            byte[] inputBytes = Encoding.UTF8.GetBytes(email);
            byte[] hash = SHA3_512.HashData(inputBytes);
            sw.Stop();
            log.LogInformation("HashEmail took {ElapsedMilliseconds}ms", sw.ElapsedMilliseconds);
            return hash;
        }

        public byte[] HashPassword(string password, byte[] salt)
        {
            Stopwatch sw = Stopwatch.StartNew();

            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] saltedPassword = new byte[salt.Length + passwordBytes.Length];

            // Kombiniere Salt + Passwort (Salt vorne)
            Buffer.BlockCopy(salt, 0, saltedPassword, 0, salt.Length);
            Buffer.BlockCopy(passwordBytes, 0, saltedPassword, salt.Length, passwordBytes.Length);

            byte[] hash = SHA3_512.HashData(saltedPassword);

            sw.Stop();
            log.LogInformation("HashPassword (SHA3-512) took {ElapsedMilliseconds}ms", sw.ElapsedMilliseconds);

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
