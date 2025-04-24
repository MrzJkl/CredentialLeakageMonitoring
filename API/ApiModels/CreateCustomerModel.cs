using System.ComponentModel.DataAnnotations;

namespace CredentialLeakageMonitoring.ApiModels
{
    public record CreateCustomerModel
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; init; } = string.Empty;

        [Required]
        public List<string> AssociatedDomains { get; init; } = [];
    }
}
