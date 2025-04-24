using Konscious.Security.Cryptography;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace CredentialLeakageMonitoring.Services
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
            Argon2id argon2 = new(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = 8,
                Iterations = 1,
                MemorySize = 65536  // 64KB
            };

            byte[] hash = argon2.GetBytes(32);

            sw.Stop();
            log.LogInformation("HashPassword took {ElapsedMilliseconds}ms", sw.ElapsedMilliseconds);

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
