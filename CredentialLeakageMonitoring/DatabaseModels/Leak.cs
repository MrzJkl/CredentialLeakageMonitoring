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
        public byte[] EmailHash { get; set; }

        [MaxLength(255)]
        public string EMailAlgorithm { get; set; }

        [MaxLength(255)]
        public string ObfuscatedPassword { get; set; }

        [MaxLength(64)]
        public byte[] PasswordHash { get; set; }

        [MaxLength(16)]
        public byte[] PasswordSalt { get; set; }

        [MaxLength(255)]
        public string PasswordAlgorithmVersion { get; set; }

        [MaxLength(255)]
        public string PasswordAlgorithm { get; set; }

        [MaxLength(255)]
        public string Domain { get; set; }

        public DateTimeOffset FirstSeen { get; set; }

        public DateTimeOffset LastSeen { get; set; }

        [ForeignKey(nameof(Customer))]
        public Guid? CustomerId { get; set; }

        public virtual Customer? Customer { get; set; }
    }
}
