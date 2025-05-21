namespace CredentialLeakageMonitoring.API.Services
{
    public class FastByteArrayComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[]? x, byte[]? y)
        {
            if (x == null || y == null)
                return false;

            return x.SequenceEqual(y);
        }

        // Computes a hash code for a byte array so it can be used as a Dictionary key
        // Thats what used by the leakLookup in IngestionService
        public int GetHashCode(byte[] obj)
        {
            if (obj == null)
                return 0;

            // Initialize hash code with a non-zero constant
            // Unchecked to prevent overflow exceptions
            unchecked
            {
                // 17 as default in .NET because it is a prime number and reduces collisions
                int hash = 17;

                // Combine hash code of each byte into a single hash value
                foreach (byte b in obj)
                {
                    // Multiply by a prime number (31) to reduce collisions
                    // 31 is computed easily in binary because of bit shifting
                    hash = hash * 31 + b;
                }

                return hash;
            }
        }
    }
}
