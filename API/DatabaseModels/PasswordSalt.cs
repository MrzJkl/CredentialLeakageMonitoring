using System.ComponentModel.DataAnnotations;

namespace CredentialLeakageMonitoring.API.DatabaseModels
{
    public class PasswordSalt
    {
        [Key]
        public Guid Id { get; init; }

        [Required]
        [MaxLength(16)]
        public byte[] Salt { get; init; } = [];
    }
}
