using CredentialLeakageMonitoring.API.ApiModels;
using CredentialLeakageMonitoring.API.Database;
using CredentialLeakageMonitoring.API.DatabaseModels;
using Microsoft.EntityFrameworkCore;

namespace CredentialLeakageMonitoring.API.Services
{
    /// <summary>
    /// Provides CRUD operations for customers and their associated domains.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="CustomerService"/> class.
    /// </remarks>
    /// <param name="dbContextFactory">The factory to create database contexts.</param>
    public class CustomerService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        /// <summary>
        /// Creates a new customer with the specified associated domains.
        /// </summary>
        /// <param name="model">The customer creation model.</param>
        /// <returns>The created customer as a DTO.</returns>
        public async Task<CustomerModel> CreateCustomer(CreateCustomerModel model)
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();

            // Find existing domains by name.
            var existingDomains = await dbContext.Domains
                .Where(d => model.AssociatedDomains.Contains(d.DomainName))
                .ToListAsync();

            var existingDomainNames = existingDomains.Select(d => d.DomainName).ToHashSet();

            // Prepare new domains that do not exist yet.
            var newDomains = model.AssociatedDomains
                .Where(dn => !existingDomainNames.Contains(dn))
                .Select(dn => new Domain { DomainName = dn })
                .ToList();

            dbContext.Domains.AddRange(newDomains);
            await dbContext.SaveChangesAsync();

            // Combine all domains for association.
            var allDomains = existingDomains.Concat(newDomains).ToList();

            var customer = new Customer
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

        /// <summary>
        /// Retrieves all customers, ordered by name, with their associated domains.
        /// </summary>
        /// <returns>A list of customer DTOs.</returns>
        public async Task<List<CustomerModel>> GetCustomers()
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();

            // Use AsNoTracking for read-only query performance.
            var customers = await dbContext.Customers
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Include(c => c.AssociatedDomains)
                .ToListAsync();

            return [.. customers.Select(c => new CustomerModel
            {
                Id = c.Id,
                Name = c.Name,
                AssociatedDomains = [.. c.AssociatedDomains.Select(d => d.DomainName)]
            })];
        }

        /// <summary>
        /// Retrieves a single customer by ID, including associated domains.
        /// </summary>
        /// <param name="id">The customer ID.</param>
        /// <returns>The customer DTO, or null if not found.</returns>
        public async Task<CustomerModel?> GetCustomer(Guid id)
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();

            var customer = await dbContext.Customers
                .AsNoTracking()
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
                AssociatedDomains = [.. customer.AssociatedDomains.Select(d => d.DomainName)]
            };
        }

        /// <summary>
        /// Updates the specified customer and their associated domains.
        /// </summary>
        /// <param name="customerModel">The customer DTO containing updated data.</param>
        /// <returns>The updated customer DTO.</returns>
        /// <exception cref="Exception">Thrown if the customer is not found.</exception>
        public async Task<CustomerModel> UpdateCustomer(CustomerModel customerModel)
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();

            var customer = await dbContext.Customers
                .Include(c => c.AssociatedDomains)
                .SingleOrDefaultAsync(c => c.Id == customerModel.Id)
                ?? throw new Exception("Customer not found");

            customer.Name = customerModel.Name;

            var domainNames = customerModel.AssociatedDomains.ToList();

            // Find and add new domains if necessary.
            var existingDomains = await dbContext.Domains
                .Where(d => domainNames.Contains(d.DomainName))
                .ToListAsync();

            var existingDomainNames = existingDomains.Select(d => d.DomainName).ToHashSet();

            var newDomains = domainNames
                .Where(dn => !existingDomainNames.Contains(dn))
                .Select(dn => new Domain { DomainName = dn })
                .ToList();

            dbContext.Domains.AddRange(newDomains);
            await dbContext.SaveChangesAsync();

            var allDomains = existingDomains.Concat(newDomains).ToList();

            customer.AssociatedDomains = allDomains;

            await dbContext.SaveChangesAsync();

            return new CustomerModel
            {
                Id = customer.Id,
                Name = customer.Name,
                AssociatedDomains = [.. allDomains.Select(d => d.DomainName)]
            };
        }

        /// <summary>
        /// Deletes a customer by ID.
        /// </summary>
        /// <param name="id">The customer ID.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown if the customer is not found.</exception>
        public async Task DeleteCustomer(Guid id)
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();

            var customer = await dbContext.Customers
                .Include(c => c.AssociatedDomains)
                .SingleOrDefaultAsync(c => c.Id == id)
                ?? throw new KeyNotFoundException("Customer not found");

            dbContext.Customers.Remove(customer);
            await dbContext.SaveChangesAsync();
        }
    }
}
