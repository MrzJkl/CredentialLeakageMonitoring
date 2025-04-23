using CredentialLeakageMonitoring.ApiModels;
using CredentialLeakageMonitoring.Database;
using Microsoft.EntityFrameworkCore;

namespace CredentialLeakageMonitoring.Services
{
    public class QueryService(ApplicationDbContext dbContext, CryptoService cryptoService)
    {
        public async Task<List<LeakModel>> SearchForLeaksByEmail(string eMail)
        {
            byte[] emailHash = cryptoService.HashEmail(eMail);

            List<DatabaseModels.Leak> leaks = await dbContext.Leaks
                .Where(l => l.EmailHash == emailHash)
                .ToListAsync()
                .ConfigureAwait(false);

            return [.. leaks.Select(l => new LeakModel
            {
                Id = l.Id,
                EmailHash = Convert.ToBase64String(l.EmailHash),
                ObfuscatedPassword = l.ObfuscatedPassword,
                FirstSeen = l.FirstSeen,
                LastSeen = l.LastSeen,
                Domain = l.Domain,
                Customer = l.Customer != null ? new CustomerModel
                {
                    Id = l.Customer.Id,
                    Name = l.Customer.Name
                } : null
            })];
        }
    }
}
