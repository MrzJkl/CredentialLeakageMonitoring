using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CredentialLeakageMonitoring.DatabaseModels
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
        public string EMailAlgorithm { get; init; } = string.Empty;

        [MaxLength(255)]
        [Required]
        public string ObfuscatedPassword { get; init; } = string.Empty;

        [MaxLength(64)]
        [Required]
        public byte[] PasswordHash { get; init; } = [];

        [MaxLength(16)]
        [Required]
        public byte[] PasswordSalt { get; init; } = [];

        [MaxLength(255)]
        [Required]
        public string PasswordAlgorithmVersion { get; init; } = string.Empty;

        [MaxLength(255)]
        [Required]
        public string PasswordAlgorithm { get; init; } = string.Empty;

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
