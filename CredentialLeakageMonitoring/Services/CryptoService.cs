using System.Security.Cryptography;
using System.Text;

namespace CredentialLeakageMonitoring.Services
{
    public class CryptoService
    {
        public byte[] HashEmail(string email)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(email);
            return SHA3_512.HashData(inputBytes);
        }


    }
}
