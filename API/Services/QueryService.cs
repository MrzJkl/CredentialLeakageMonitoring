using CredentialLeakageMonitoring.API.ApiModels;
using CredentialLeakageMonitoring.API.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace CredentialLeakageMonitoring.API.Services
{
    public class QueryService(IDbContextFactory<ApplicationDbContext> dbContextFactory, CryptoService cryptoService)
    {
        public async Task<List<LeakModel>> SearchForLeaksByEmail(string eMail)
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();
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
                AssociatedCustomers = [.. l.AssociatedCustomers.Select(c => new CustomerModel
                {
                    Id = c.Id,
                    Name = c.Name
                })],
            })];
        }

        public async Task<List<LeakModel>> SearchForLeaksByCustomerId(Guid customerId)
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();

            DatabaseModels.Customer customer = await dbContext.Customers
                .Include(c => c.AssociatedDomains)
                .SingleOrDefaultAsync(c => c.Id == customerId)
                .ConfigureAwait(false) ?? throw new Exception($"Customer with ID {customerId} not found.");

            List<string> domainNames = [.. customer.AssociatedDomains.Select(d => d.DomainName)];

            List<DatabaseModels.Leak> leaks = await dbContext.Leaks
                .Include(l => l.AssociatedCustomers)
                .Where(l => domainNames.Contains(l.Domain))
                .ToListAsync()
                .ConfigureAwait(false);

            foreach (DatabaseModels.Leak leak in leaks)
            {
                if (!leak.AssociatedCustomers.Any(c => c.Id == customerId))
                {
                    leak.AssociatedCustomers.Add(customer);

                    await dbContext.SaveChangesAsync();
                }
            }

            return [.. leaks.Select(l => new LeakModel
            {
                Id = l.Id,
                EmailHash = Convert.ToBase64String(l.EmailHash),
                ObfuscatedPassword = l.ObfuscatedPassword,
                FirstSeen = l.FirstSeen,
                LastSeen = l.LastSeen,
                Domain = l.Domain,
                AssociatedCustomers = [.. l.AssociatedCustomers.Select(c => new CustomerModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    AssociatedDomains = [.. c.AssociatedDomains.Select(d => d.DomainName)],
                })],
            })];
        }
    }
}
