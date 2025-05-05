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
    public class IngestionService(IDbContextFactory<ApplicationDbContext> dbContextFactory, CryptoService cryptoService, ILogger<IngestionService> log)
    {
        private const int MaxChunks = 20;

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
            log.LogInformation("Precomputation of {Count} hashes and domains took {Elapsed}", emailHashes.Count, sw.Elapsed);

            sw.Restart();
            // Load all existing leaks and domains for the chunk in advance.
            var existingAccounts = await dbContext.Leaks
                .AsNoTracking()
                .Include(l => l.PasswordSalt)
                .Where(l => emailHashes.Contains(l.EmailHash))
                .ToListAsync();

            var domains = await dbContext.Domains
                .AsNoTracking()
                .Include(d => d.AssociatedByCustomers)
                .Where(d => domainNames.Contains(d.DomainName))
                .ToListAsync();

            sw.Stop();
            log.LogInformation("Loading {Count} Database took {Elapsed}", existingAccounts.Count, sw.Elapsed);

            var newLeaks = new List<Leak>();
            var leakToUpdateIds = new List<Guid>();

            sw.Restart();

            foreach (var record in chunk)
            {
                var emailHash = cryptoService.HashEmail(record.Email);
                var leaksForEmail = existingAccounts
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
                            leakToUpdateIds.Add(existingLeak.Id);
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
                    PasswordSaltId = newSalt.Id,
                    Domain = domainName,
                    AssociatedCustomers = customers,
                    FirstSeen = DateTimeOffset.UtcNow,
                    LastSeen = DateTimeOffset.UtcNow
                };

                newLeaks.Add(newLeak);
            }

            sw.Stop();
            log.LogInformation("Processing took {Elapsed}", sw.Elapsed);

            sw.Restart();
            var now = DateTimeOffset.UtcNow;
            await dbContext.Leaks
                .Where(l => leakToUpdateIds.Contains(l.Id))
                .ExecuteUpdateAsync(l => l
                    .SetProperty(leak => leak.LastSeen, now));

            sw.Stop();
            log.LogInformation("Updating {Count} leaks took {Elapsed}", leakToUpdateIds.Count, sw.Elapsed);

            sw.Restart();
            // Add all new leaks in one batch.
            if (newLeaks.Count > 0)
            {
                await dbContext.BulkInsertAsync(newLeaks.Select(l => l.PasswordSalt));
                await dbContext.BulkInsertAsync(newLeaks);
            }

            sw.Stop();
            log.LogInformation("Inserting {Count} new leaks took {Elapsed}", newLeaks.Count, sw.Elapsed);

            await dbContext.DisposeAsync();
        }
    }
}
