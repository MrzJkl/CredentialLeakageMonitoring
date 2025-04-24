using CredentialLeakageMonitoring.DatabaseModels;
using Microsoft.EntityFrameworkCore;

namespace CredentialLeakageMonitoring.Database
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<Leak> Leaks { get; private set; }

        public DbSet<Customer> Customers { get; private set; }

        public DbSet<Domain> Domains { get; private set; }
    }
}
