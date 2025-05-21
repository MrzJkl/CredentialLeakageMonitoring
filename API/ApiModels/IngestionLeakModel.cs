namespace CredentialLeakageMonitoring.API.ApiModels
{
    public record IngestionLeakModel(string email, string plaintextPassword)
    {
        public string Email { get; private set; } = email?.Trim().ToLowerInvariant() ?? string.Empty;

        public string PlaintextPassword { get; private set; } = plaintextPassword?.Trim() ?? string.Empty;
    }
}
