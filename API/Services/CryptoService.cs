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
        public const string TestSecret = "b6166cdf-2d18-4ab9-9425-6a0ce9561603";

        /// <summary>
        /// Thread safe
        /// </summary>
        public static byte[] HashEmail(string email)
        {
            email = email.Trim().ToLowerInvariant();
            byte[] inputBytes = Encoding.UTF8.GetBytes(email);
            byte[] hash = SHA512.HashData(inputBytes);
            return hash;
        }

        public byte[] EncryptPassword(string data, byte[] key)
        {
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            aes.GenerateIV();
            byte[] iv = aes.IV;

            byte[] plainBytes = Encoding.UTF8.GetBytes(data);

            using ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            byte[] result = new byte[iv.Length + encrypted.Length];
            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
            Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);

            return result;
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

        public byte[] DeriveKey(string email, string secret)
        {
            string input = email.Trim().ToLowerInvariant() + ":" + secret;
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = SHA256.HashData(inputBytes);

            return hash;
        }
    }
}
