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
        public Guid Id { get; private set; }

        public byte[] EmailHash { get; private set; }

        public string ObfuscatedPassword { get; private set; }

        public byte[] PasswordHash { get; private set; }

        [MaxLength(255)]
        public string Domain { get; private set; }

        public DateTimeOffset FirstSeen { get; private set; }

        public DateTimeOffset LastSeen { get; private set; }

        [ForeignKey(nameof(Customer))]
        public Guid? CustomerId { get; private set; }

        public virtual Customer? Customer { get; private set; }
    }
}
