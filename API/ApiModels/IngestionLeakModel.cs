namespace CredentialLeakageMonitoring.API.ApiModels
{
    public record IngestionLeakModel
    {
        public string Email { get; init; } = string.Empty;

        public string PlaintextPassword { get; init; } = string.Empty;
    }
}
