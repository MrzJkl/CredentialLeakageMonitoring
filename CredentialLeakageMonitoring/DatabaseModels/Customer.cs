using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CredentialLeakageMonitoring.DatabaseModels
{
    [Index(nameof(Name), IsUnique = true)]
    public class Customer
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public virtual List<Domain> AssociatedDomains { get; set; } = [];

        public virtual List<Leak> AssociatedLeaks { get; set; } = [];
    }
}
