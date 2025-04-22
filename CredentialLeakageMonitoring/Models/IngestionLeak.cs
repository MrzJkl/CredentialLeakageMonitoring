namespace CredentialLeakageMonitoring.Models
{
    public record IngestionLeak
    {
        public string EMail { get; private set; }

        public string Password { get; private set; }
    }
}
