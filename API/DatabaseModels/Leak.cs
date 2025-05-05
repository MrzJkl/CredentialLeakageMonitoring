using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [MaxLength(255)]
        [Required]
        public string ObfuscatedPassword { get; init; } = string.Empty;

        [MaxLength(64)]
        [Required]
        public byte[] PasswordHash { get; init; } = [];

        [MaxLength(255)]
        [Required]
        public string Domain { get; init; } = string.Empty;

        [Required]
        public DateTimeOffset FirstSeen { get; init; }

        [Required]
        public DateTimeOffset LastSeen { get; set; }

        [Required]
        [ForeignKey(nameof(PasswordSalt))]
        public Guid PasswordSaltId { get; set; }

        public virtual List<Customer> AssociatedCustomers { get; set; } = [];

        public virtual PasswordSalt PasswordSalt { get; set; } = new();
    }
}
