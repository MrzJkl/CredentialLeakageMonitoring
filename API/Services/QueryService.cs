using CredentialLeakageMonitoring.API.ApiModels;
using CredentialLeakageMonitoring.API.Database;
using Microsoft.EntityFrameworkCore;

namespace CredentialLeakageMonitoring.API.Services
{
    /// <summary>
    /// Provides methods to query for credential leaks by email or customer.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="QueryService"/> class.
    /// </remarks>
    /// <param name="dbContextFactory">The factory to create database contexts.</param>
    /// <param name="cryptoService">The service for hashing emails.</param>
    public class QueryService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        /// <summary>
        /// Searches for leaks by the specified email address.
        /// </summary>
        /// <param name="eMail">The email address to search for.</param>
        /// <returns>A list of leaks matching the email hash.</returns>
        public async Task<List<LeakModel>> SearchForLeaksByEmail(string eMail)
        {
            using ApplicationDbContext dbContext = await dbContextFactory.CreateDbContextAsync();
            byte[] emailHash = CryptoService.HashEmail(eMail);

            // Query leaks matching the hashed email.
            List<LeakModel> leaks = await dbContext.Leaks
                .AsNoTracking()
                .Where(l => l.EmailHash == emailHash)
                .Select(l => new LeakModel
                {
                    Id = l.Id,
                    EmailHash = Convert.ToBase64String(l.EmailHash),
                    FirstSeen = l.FirstSeen,
                    LastSeen = l.LastSeen,
                    Domain = l.Domain,
                    AssociatedCustomers = l.AssociatedCustomers
                        .Select(c => new CustomerModel
                        {
                            Id = c.Id,
                            Name = c.Name
                        }).ToList()
                })
                .ToListAsync()
                .ConfigureAwait(false);

            return leaks;
        }

        /// <summary>
        /// Searches for leaks associated with the given customer ID.
        /// Links the customer to found leaks if not already associated.
        /// </summary>
        /// <param name="customerId">The customer ID.</param>
        /// <returns>A list of leaks associated with the customer's domains.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the customer does not exist.</exception>
        public async Task<List<LeakModel>> SearchForLeaksByCustomerId(Guid customerId)
        {
            using ApplicationDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

            DatabaseModels.Customer customer = await dbContext.Customers
                .Include(c => c.AssociatedDomains)
                .SingleOrDefaultAsync(c => c.Id == customerId)
                .ConfigureAwait(false) ?? throw new KeyNotFoundException($"Customer with ID {customerId} not found.");
            List<string> domainNames = [.. customer.AssociatedDomains.Select(d => d.DomainName)];

            // Find leaks for the customer's domains.
            var leaks = await dbContext.Leaks
                .Include(l => l.AssociatedCustomers)
                .AsNoTracking()
                .Where(l => domainNames.Contains(l.Domain))
                .OrderByDescending(l => l.FirstSeen)
                .Select(l => new
                {
                    Leak = l,
                    AlreadyAssociated = l.AssociatedCustomers.Any(c => c.Id == customerId)
                })
                .ToListAsync()
                .ConfigureAwait(false);

            // Associate customer with leaks if not already linked.
            List<DatabaseModels.Leak> leaksToAssociate = [.. leaks
                .Where(x => !x.AlreadyAssociated)
                .Select(x => x.Leak)];

            if (leaksToAssociate.Count != 0)
            {
                List<DatabaseModels.Leak> trackedLeaks = await dbContext.Leaks
                    .Where(l => leaksToAssociate.Select(lt => lt.Id).Contains(l.Id))
                    .Include(l => l.AssociatedCustomers)
                    .ToListAsync()
                    .ConfigureAwait(false);

                foreach (DatabaseModels.Leak? leak in trackedLeaks)
                {
                    leak.AssociatedCustomers.Add(customer);
                }
                await dbContext.SaveChangesAsync();
            }

            // Return all relevant leaks as DTOs.
            List<LeakModel> result = await dbContext.Leaks
                .AsNoTracking()
                .Where(l => domainNames.Contains(l.Domain))
                .OrderByDescending(l => l.FirstSeen)
                .Select(l => new LeakModel
                {
                    Id = l.Id,
                    EmailHash = Convert.ToBase64String(l.EmailHash),
                    FirstSeen = l.FirstSeen,
                    LastSeen = l.LastSeen,
                    Domain = l.Domain,
                    AssociatedCustomers = l.AssociatedCustomers
                        .Select(c => new CustomerModel
                        {
                            Id = c.Id,
                            Name = c.Name,
                            AssociatedDomains = c.AssociatedDomains.Select(d => d.DomainName).ToList()
                        }).ToList()
                })
                .ToListAsync()
                .ConfigureAwait(false);

            return result;
        }
    }
}
