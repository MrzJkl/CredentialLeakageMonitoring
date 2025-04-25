using System.ComponentModel.DataAnnotations;

namespace CredentialLeakageMonitoring.API.ApiModels
{
    public record CustomerModel
    {
        [Required]
        public Guid Id { get; init; }

        [Required]
        public string Name { get; init; } = string.Empty;

        [Required]
        public List<string> AssociatedDomains { get; init; } = [];
    }
}
