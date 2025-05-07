using System.Security.Cryptography;
using System.Text;

namespace CredentialLeakageMonitoring.API.Services
{
    /// <summary>
    /// This class is thread-safe because it does not use shared (mutable) fields or instance variables.
    /// </summary>
    public static class CryptoService
    {
        public const string TestSecret = "b6166cdf-2d18-4ab9-9425-6a0ce9561603";

        /// <summary>
        /// Thread safe
        /// </summary>
        public static byte[] HashEmail(string email)
        {
            email = email.Trim().ToLowerInvariant();
            byte[] inputBytes = Encoding.UTF8.GetBytes(email);
            byte[] hash = SHA256.HashData(inputBytes);
            return hash;
        }

        public static bool ArePasswordsEqual(byte[] passwordCipher, string plaintextPassword, byte[] key)
        {
            string decryptedPassword = DecryptPassword(passwordCipher, key);
            return string.Equals(decryptedPassword, plaintextPassword, StringComparison.Ordinal);
        }

        public static string DecryptPassword(byte[] cipherData, byte[] key)
        {
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            byte[] iv = new byte[16];
            Buffer.BlockCopy(cipherData, 0, iv, 0, iv.Length);
            aes.IV = iv;

            int cipherTextLength = cipherData.Length - iv.Length;
            byte[] cipherText = new byte[cipherTextLength];
            Buffer.BlockCopy(cipherData, iv.Length, cipherText, 0, cipherTextLength);

            using ICryptoTransform decryptor = aes.CreateDecryptor();
            byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }

        public static byte[] EncryptPassword(string data, byte[] key)
        {
            using Aes aes = Aes.Create();
        
            aes.GenerateIV();
            aes.Key = key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            byte[] plainBytes = Encoding.UTF8.GetBytes(data);

            using ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            byte[] result = new byte[aes.IV.Length + encrypted.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(encrypted, 0, result, aes.IV.Length, encrypted.Length);

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
