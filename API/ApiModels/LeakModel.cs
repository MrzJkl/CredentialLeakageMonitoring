using System.ComponentModel.DataAnnotations;

namespace CredentialLeakageMonitoring.API.ApiModels
{
    public record LeakModel
    {
        [Required]
        public Guid Id { get; init; }

        [Required]
        public string EmailHash { get; init; } = string.Empty;

        [Required]
        public string ObfuscatedPassword { get; init; } = string.Empty;

        [Required]
        public DateTimeOffset FirstSeen { get; init; }

        [Required]
        public DateTimeOffset LastSeen { get; init; }

        [Required]
        public string Domain { get; init; } = string.Empty;

        [Required]
        public List<CustomerModel> AssociatedCustomers { get; init; } = [];
    }
}
