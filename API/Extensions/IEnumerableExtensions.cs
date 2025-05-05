namespace CredentialLeakageMonitoring.API.Extensions
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Splits a collection into chunks of the specified size.
        /// </summary>
        public static IEnumerable<List<T>> Chunk<T>(this IEnumerable<T> source, int size)
        {
            List<T> chunk = new(size);
            foreach (T? item in source)
            {
                chunk.Add(item);
                if (chunk.Count >= size)
                {
                    yield return chunk;
                    chunk = new(size);
                }
            }
            if (chunk.Count != 0)
                yield return chunk;
        }
    }
}
