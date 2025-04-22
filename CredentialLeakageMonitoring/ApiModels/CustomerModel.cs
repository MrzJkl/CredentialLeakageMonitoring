namespace CredentialLeakageMonitoring.ApiModels
{
    public record CustomerModel
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = string.Empty;
    }
}
