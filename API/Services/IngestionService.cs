using CredentialLeakageMonitoring.API.ApiModels;
using CredentialLeakageMonitoring.API.Database;
using CredentialLeakageMonitoring.API.DatabaseModels;
using CsvHelper;
using CsvHelper.Configuration;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;

namespace CredentialLeakageMonitoring.API.Services
{
    /// <summary>
    /// Provides high-performance ingestion of leaked credentials from CSV streams.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="IngestionService"/> class.
    /// </remarks>
    public class IngestionService(IDbContextFactory<ApplicationDbContext> dbContextFactory, ILogger<IngestionService> log)
    {
        private const int MaxChunks = 30;

        /// <summary>
        /// Ingests leaks from a CSV stream in batches.
        /// </summary>
        /// <param name="csvStream">The CSV file stream.</param>
        /// <param name="chunkSize">The batch size for processing.</param>
        public async Task IngestCsvAsync(Stream csvStream)
        {
            Stopwatch sw = Stopwatch.StartNew();
            List<IngestionLeakModel> records = new();

            // Read all records from the CSV stream.
            using StreamReader reader = new(csvStream);
            using CsvReader csv = new(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = false,
                TrimOptions = TrimOptions.Trim,
            });

            await foreach (IngestionLeakModel record in csv.GetRecordsAsync<IngestionLeakModel>())
            {
                records.Add(record);
            }

            records = records
                .Distinct()
                .ToList();

            int chunkSize = records.Count / MaxChunks;

            // Process records in parallel chunks.
            IEnumerable<IngestionLeakModel[]> chunks = records.Chunk(chunkSize);
            IEnumerable<Task> tasks = chunks.Select(chunk => ProcessChunkAsync(chunk));
            await Task.WhenAll(tasks);
            sw.Stop();
            log.LogInformation("Ingestion of {Count} leaks took {Elapsed}", records.Count, sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Processes a batch of leaks.
        /// </summary>
        private async Task ProcessChunkAsync(IEnumerable<IngestionLeakModel> chunk)
        {
            await using ApplicationDbContext dbContext = dbContextFactory.CreateDbContext();
#if DEBUG
            Stopwatch sw = Stopwatch.StartNew();
#endif
            // Precompute hashes and domains for all records in the chunk.
            Dictionary<string, (byte[] Hash, string Domain)> emailInfos = chunk
                .AsParallel()
                .DistinctBy(l => l.Email)
                .ToDictionary(
                    r => r.Email,
                    r => (
                        Hash: CryptoService.HashEmail(r.Email),
                        Domain: Helper.GetDomainFromEmail(r.Email)
                    )
                );

            byte[][] emailHashes = emailInfos.Values
                .AsParallel()
                .Select(v => v.Hash)
                .ToArray();
            string[] domainNames = emailInfos.Values
                .AsParallel()
                .Select(v => v.Domain)
                .ToArray();

#if DEBUG
            sw.Stop();
            log.LogInformation("Precomputation of {Count} hashes and domains took {Elapsed}", emailInfos.Count, sw.Elapsed);

            sw.Restart();
#endif
            // Load all existing leaks and domains for the chunk in advance.
            List<Leak> leaks = await dbContext.Leaks
                .AsNoTracking()
                .Where(l => emailHashes.Contains(l.EmailHash))
                .ToListAsync();

            // Group leaks by email hash for faster lookup.
            Dictionary<byte[], List<Leak>> leakLookup = leaks
                .GroupBy(l => l.EmailHash, new FastByteArrayComparer())
                .ToDictionary(g => g.Key, g => g.ToList(), new FastByteArrayComparer());

            List<Domain> domains = await dbContext.Domains
                .AsNoTracking()
                .Include(d => d.AssociatedByCustomers)
                .Where(d => domainNames.Contains(d.DomainName))
                .ToListAsync();

#if DEBUG
            sw.Stop();
            log.LogInformation("Loading {Count} Database took {Elapsed}", existingAccounts.Count, sw.Elapsed);
            sw.Restart();
#endif
            List<Leak> newLeaks = [];
            List<Guid> leaksToUpdateIds = [];

            foreach (IngestionLeakModel record in chunk)
            {
                byte[] emailHash = emailInfos[record.Email].Hash;
                bool foundLeaks = leakLookup.TryGetValue(emailHash, out List<Leak>? leaksForEmail);
                bool foundMatchingLeak = false;

                if (foundLeaks && leaksForEmail!.Count != 0)
                {

                    foreach (Leak? existingLeak in leaksForEmail)
                    {
                        byte[] salt = existingLeak.PasswordSalt;
                        byte[] passwordHash = CryptoService.HashPassword(record.PlaintextPassword, salt);

                        if (passwordHash.SequenceEqual(existingLeak.PasswordHash))
                        {
                            leaksToUpdateIds.Add(existingLeak.Id);
                            foundMatchingLeak = true;
                            break;
                        }
                    }
                }

                if (foundMatchingLeak) continue;

                // Prepare new leak for insertion.
                byte[] saltValue = CryptoService.GenerateRandomSalt();
                byte[] passwordHashWithSalt = CryptoService.HashPassword(record.PlaintextPassword, saltValue);
                string domainName = emailInfos[record.Email].Domain;
                Domain? domain = domains.SingleOrDefault(d => d.DomainName == domainName);

                List<Customer> customers = domain?.AssociatedByCustomers ?? [];

                Leak newLeak = new()
                {
                    Id = Guid.NewGuid(),
                    EmailHash = emailHash,
                    PasswordHash = passwordHashWithSalt,
                    PasswordSalt = saltValue,
                    Domain = domainName,
                    AssociatedCustomers = customers,
                    FirstSeen = DateTimeOffset.UtcNow,
                    LastSeen = DateTimeOffset.UtcNow
                };

                newLeaks.Add(newLeak);
            }
#if DEBUG
            sw.Stop();
            log.LogInformation("Processing took {Elapsed}", sw.Elapsed);
            sw.Restart();
#endif

            DateTimeOffset now = DateTimeOffset.UtcNow;
            await dbContext.Leaks
                .Where(l => leaksToUpdateIds.Contains(l.Id))
                .ExecuteUpdateAsync(l => l
                    .SetProperty(leak => leak.LastSeen, now));

#if DEBUG
            sw.Stop();
            log.LogInformation("Updating {Count} leaks took {Elapsed}", leaksToUpdateIds.Count, sw.Elapsed);
            sw.Restart();
#endif

            // Add all new leaks in one batch.
            if (newLeaks.Count > 0)
            {
                await dbContext.BulkInsertAsync(newLeaks);
            }

#if DEBUG
            sw.Stop();
            log.LogInformation("Inserting {Count} new leaks took {Milliseconds}", newLeaks.Count, sw.ElapsedMilliseconds);
#endif
            await dbContext.DisposeAsync();
        }
    }
}
