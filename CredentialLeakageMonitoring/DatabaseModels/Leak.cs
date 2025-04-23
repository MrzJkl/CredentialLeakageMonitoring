using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public byte[] EmailHash { get; set; }

        [MaxLength(255)]
        [Required]
        public string EMailAlgorithm { get; set; }

        [MaxLength(255)]
        [Required]
        public string ObfuscatedPassword { get; set; }

        [MaxLength(64)]
        [Required]
        public byte[] PasswordHash { get; set; }

        [MaxLength(16)]
        [Required]
        public byte[] PasswordSalt { get; set; }

        [MaxLength(255)]
        [Required]
        public string PasswordAlgorithmVersion { get; set; }

        [MaxLength(255)]
        [Required]
        public string PasswordAlgorithm { get; set; }

        [MaxLength(255)]
        [Required]
        public string Domain { get; set; }

        [Required]
        public DateTimeOffset FirstSeen { get; set; }

        [Required]
        public DateTimeOffset LastSeen { get; set; }

        [ForeignKey(nameof(Customer))]
        public Guid? CustomerId { get; set; }

        public virtual Customer? Customer { get; set; }
    }
}
