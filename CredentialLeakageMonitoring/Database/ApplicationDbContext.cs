using CredentialLeakageMonitoring.DatabaseModels;
using Microsoft.EntityFrameworkCore;

namespace CredentialLeakageMonitoring.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Leak> Leaks { get; set; }

        public DbSet<Customer> Customers { get; set; }
    }
}
