using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CredentialLeakageMonitoring.API.DatabaseModels
{
    [Index(nameof(DomainName), IsUnique = true)]
    public class Domain
    {
        [Key]
        public Guid Id { get; init; }

        [Required]
        [MaxLength(255)]
        public string DomainName { get; init; } = string.Empty;

        public virtual List<Customer> AssociatedByCustomers { get; init; } = [];
    }
}
