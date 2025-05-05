using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CredentialLeakageMonitoring.API.DatabaseModels
{
    [Index(nameof(EmailHash))]
    [Index(nameof(Domain))]
    public class Leak
    {
        [Key]
        public Guid Id { get; init; }

        [MaxLength(64)]
        [Required]
        public byte[] EmailHash { get; init; } = [];

        [Required]
        [MaxLength(64)]
        public byte[] PasswordHash { get; init; } = [];

        [Required]
        [MaxLength(16)]
        public byte[] PasswordSalt { get; init; } = [];

        [MaxLength(255)]
        [Required]
        public string Domain { get; init; } = string.Empty;

        [Required]
        public DateTimeOffset FirstSeen { get; init; }

        [Required]
        public DateTimeOffset LastSeen { get; set; }

        public virtual List<Customer> AssociatedCustomers { get; set; } = [];
    }
}
