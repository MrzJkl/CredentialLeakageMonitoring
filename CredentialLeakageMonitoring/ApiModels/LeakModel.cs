namespace CredentialLeakageMonitoring.ApiModels
{
    public record LeakModel
    {
        public Guid Id { get; init; }

        public string EmailHash { get; init; } = string.Empty;

        public string ObfuscatedPassword { get; init; } = string.Empty;

        public DateTimeOffset FirstSeen { get; init; }

        public DateTimeOffset LastSeen { get; init; }

        public string Domain { get; init; } = string.Empty;

        public CustomerModel? Customer { get; init; } = null;
    }
}
