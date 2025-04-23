using System.ComponentModel.DataAnnotations;

namespace CredentialLeakageMonitoring.DatabaseModels
{
    public class Customer
    {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }
    }
}
