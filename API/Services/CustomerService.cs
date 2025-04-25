using CredentialLeakageMonitoring.API.ApiModels;
using CredentialLeakageMonitoring.API.Database;
using CredentialLeakageMonitoring.API.DatabaseModels;
using Microsoft.EntityFrameworkCore;

namespace CredentialLeakageMonitoring.API.Services
{
    public class CustomerService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        public async Task<CustomerModel> CreateCustomer(CreateCustomerModel model)
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();
            List<Domain> existingDomains = await dbContext.Domains
                .Where(d => model.AssociatedDomains.Contains(d.DomainName))
                .ToListAsync();

            HashSet<string> existingDomainNames = [.. existingDomains.Select(d => d.DomainName)];
            List<Domain> newDomains = [.. model.AssociatedDomains
                .Where(dn => !existingDomainNames.Contains(dn))
                .Select(dn => new Domain { DomainName = dn })];

            dbContext.Domains.AddRange(newDomains);
            await dbContext.SaveChangesAsync();

            List<Domain> allDomains = [.. existingDomains, .. newDomains];

            Customer customer = new()
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                AssociatedDomains = allDomains
            };

            dbContext.Customers.Add(customer);
            await dbContext.SaveChangesAsync();

            return new CustomerModel
            {
                Id = customer.Id,
                Name = customer.Name,
                AssociatedDomains = [.. allDomains.Select(d => d.DomainName)]
            };
        }

        public async Task<List<CustomerModel>> GetCustomers()
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();

            List<Customer> customers = await dbContext.Customers
                .OrderBy(c => c.Name)
                .Include(c => c.AssociatedDomains)
                .ToListAsync();

            return [.. customers
                .Select(c => new CustomerModel
            {
                Id = c.Id,
                Name = c.Name,
                AssociatedDomains = [.. c.AssociatedDomains.Select(d => d.DomainName)]

            })];
        }

        public async Task<CustomerModel?> GetCustomer(Guid id)
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();

            Customer? customer = await dbContext.Customers
                .Include(c => c.AssociatedDomains)
                .SingleOrDefaultAsync(c => c.Id == id);

            if (customer is null)
            {
                return null;
            }

            return new CustomerModel
            {
                Id = customer.Id,
                Name = customer.Name,
                AssociatedDomains = [.. customer.AssociatedDomains.Select(d => d.DomainName)],
            };
        }

        public async Task<CustomerModel> UpdateCustomer(CustomerModel customerModel)
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();

            Customer? customer = await dbContext.Customers
                .Include(c => c.AssociatedDomains)
                .SingleOrDefaultAsync(c => c.Id == customerModel.Id)
                ?? throw new Exception("Customer not found");

            customer.Name = customerModel.Name;

            List<string> domainNames = [.. customerModel.AssociatedDomains];

            List<Domain> existingDomains = await dbContext.Domains
                .Where(d => domainNames.Contains(d.DomainName))
                .ToListAsync();

            HashSet<string> existingDomainNames = [.. existingDomains.Select(d => d.DomainName)];
            List<Domain> newDomains = [.. domainNames
                .Where(dn => !existingDomainNames.Contains(dn))
                .Select(dn => new Domain { DomainName = dn })];

            dbContext.Domains.AddRange(newDomains);
            await dbContext.SaveChangesAsync();

            List<Domain> allDomains = [.. existingDomains, .. newDomains];

            customer.AssociatedDomains = allDomains;

            await dbContext.SaveChangesAsync();

            return new CustomerModel
            {
                Id = customer.Id,
                Name = customer.Name,
                AssociatedDomains = [.. allDomains.Select(d => d.DomainName)]
            };
        }

        public async Task DeleteCustomer(Guid id)
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();

            Customer? customer = await dbContext.Customers
                .Include(c => c.AssociatedDomains)
                .SingleOrDefaultAsync(c => c.Id == id)
                ?? throw new Exception("Customer not found");
            dbContext.Customers.Remove(customer);
            await dbContext.SaveChangesAsync();
        }
    }
}
