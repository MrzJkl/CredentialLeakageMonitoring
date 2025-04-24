using System.Net.Mail;

namespace CredentialLeakageMonitoring
{
    public static class Helper
    {
        private const char ObfuscationChar = '*';

        public static string ObfuscatePassword(string plainPassword)
        {
            if (string.IsNullOrEmpty(plainPassword))
                return string.Empty;

            char[] result = new char[plainPassword.Length];

            for (int i = 0; i < plainPassword.Length; i++)
            {
                result[i] = i % 2 == 0 ? ObfuscationChar : plainPassword[i];
            }

            return new string(result);
        }

        public static string GetDomainFromEmail(string email)
        {
            MailAddress mail = new(email);
            return mail.Host.ToLowerInvariant();
        }
    }
}
