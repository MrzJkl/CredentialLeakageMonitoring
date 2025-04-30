using CredentialLeakageMonitoring.API.ApiModels;
using CredentialLeakageMonitoring.API.Database;
using CredentialLeakageMonitoring.API.DatabaseModels;
using CsvHelper;
using CsvHelper.Configuration;
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
    public class IngestionService(IDbContextFactory<ApplicationDbContext> dbContextFactory, CryptoService cryptoService, ILogger<IngestionService> log)
    {
        private const int MaxChunks = 30;

        /// <summary>
        /// Ingests leaks from a CSV stream in batches.
        /// </summary>
        /// <param name="csvStream">The CSV file stream.</param>
        /// <param name="chunkSize">The batch size for processing.</param>
        public async Task IngestCsvAsync(Stream csvStream)
        {
            var sw = Stopwatch.StartNew();
            var records = new List<IngestionLeakModel>();

            // Read all records from the CSV stream.
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = false,
                TrimOptions = TrimOptions.Trim,
            });

            await foreach (var record in csv.GetRecordsAsync<IngestionLeakModel>())
            {
                records.Add(record);
            }

            var chunkSize = records.Count / MaxChunks;

            // Process records in parallel chunks.
            var chunks = records.Chunk(chunkSize);
            var tasks = chunks.Select(chunk => ProcessChunkAsync(chunk));
            await Task.WhenAll(tasks);

            sw.Stop();
            log.LogInformation("Ingestion of {Count} leaks took {Elapsed}", records.Count, sw.Elapsed);
        }

        /// <summary>
        /// Processes a batch of leaks.
        /// </summary>
        private async Task ProcessChunkAsync(IEnumerable<IngestionLeakModel> chunk)
        {
            await using var dbContext = dbContextFactory.CreateDbContext();
            var sw = Stopwatch.StartNew();
            // Precompute hashes and domains for all records in the chunk.
            var emailHashes = chunk
                .AsParallel()
                .Select(r => cryptoService.HashEmail(r.Email))
                .ToList();
            var domainNames = chunk
                .AsParallel()
                .Select(r => Helper.GetDomainFromEmail(r.Email))
                .ToList();

            sw.Stop();
            log.LogInformation("Precomputation of hashes and domains took {Elapsed}", sw.Elapsed);

            // Load all existing leaks and domains for the chunk in advance.
            var existingLeaks = await dbContext.Leaks
                .AsNoTracking()
                .Include(l => l.PasswordSalt)
                .Where(l => emailHashes.Contains(l.EmailHash))
                .ToListAsync();

            var domains = await dbContext.Domains
                .AsNoTracking()
                .Include(d => d.AssociatedByCustomers)
                .Where(d => domainNames.Contains(d.DomainName))
                .ToListAsync();

            var newLeaks = new List<Leak>();

            foreach (var record in chunk)
            {
                var emailHash = cryptoService.HashEmail(record.Email);
                var leaksForEmail = existingLeaks
                    .Where(l => l.EmailHash.SequenceEqual(emailHash))
                    .ToList();

                bool foundMatchingLeak = false;

                if (leaksForEmail.Count != 0)
                {
                    var salt = leaksForEmail.First().PasswordSalt.Salt;
                    var passwordHash = cryptoService.HashPassword(record.PlaintextPassword, salt);

                    foreach (var existingLeak in leaksForEmail)
                    {
                        if (passwordHash.SequenceEqual(existingLeak.PasswordHash))
                        {
                            // Update LastSeen for existing leak.
                            existingLeak.LastSeen = DateTimeOffset.UtcNow;
                            dbContext.Leaks.Update(existingLeak);
                            foundMatchingLeak = true;
                            break;
                        }
                    }
                }

                if (foundMatchingLeak) continue;

                // Prepare new leak for insertion.
                var saltValue = cryptoService.GenerateRandomSalt();
                var passwordHashWithSalt = cryptoService.HashPassword(record.PlaintextPassword, saltValue);
                var domainName = Helper.GetDomainFromEmail(record.Email);
                var domain = domains.SingleOrDefault(d => d.DomainName == domainName);

                var customers = domain?.AssociatedByCustomers ?? [];
                var newSalt = new PasswordSalt
                { 
                    Id = Guid.NewGuid(), 
                    Salt = saltValue 
                };

                var newLeak = new Leak
                {
                    Id = Guid.NewGuid(),
                    EmailHash = emailHash,
                    ObfuscatedPassword = Helper.ObfuscatePassword(record.PlaintextPassword),
                    PasswordHash = passwordHashWithSalt,
                    PasswordSalt = newSalt,
                    PasswordAlgorithmVersion = CryptoService.AlgorithmVersionForPassword,
                    PasswordAlgorithm = CryptoService.AlgorithmForPassword,
                    EMailAlgorithm = CryptoService.AlgorithmForEmail,
                    Domain = domainName,
                    AssociatedCustomers = customers,
                    FirstSeen = DateTimeOffset.UtcNow,
                    LastSeen = DateTimeOffset.UtcNow
                };

                newLeaks.Add(newLeak);
            }

            // Add all new leaks in one batch.
            if (newLeaks.Count > 0)
            {
                dbContext.Leaks.AddRange(newLeaks);
            }

            await dbContext.SaveChangesAsync();
            await dbContext.DisposeAsync();
        }
    }
}
