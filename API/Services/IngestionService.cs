using CredentialLeakageMonitoring.ApiModels;
using CredentialLeakageMonitoring.Database;
using CredentialLeakageMonitoring.DatabaseModels;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;

namespace CredentialLeakageMonitoring.Services
{
    public class IngestionService(IDbContextFactory<ApplicationDbContext> dbContextFactory, CryptoService cryptoService, ILogger<IngestionService> log)
    {
        public async Task IngestCsvAsync(Stream csvStream, int chunkSize = 100)
        {
            var sw = Stopwatch.StartNew();
            var records = new List<IngestionLeakModel>();

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

            var chunks = Chunk(records, chunkSize);
            var tasks = chunks.Select(chunk => ProcessChunkAsync(chunk));
            await Task.WhenAll(tasks);

            sw.Stop();
            log.LogInformation("Ingestion of {Count} leaks took {Elapsed}", records.Count, sw.Elapsed);
        }

        private async Task ProcessChunkAsync(List<IngestionLeakModel> chunk)
        {
            await using var dbContext = dbContextFactory.CreateDbContext();

            foreach (var record in chunk)
            {
                byte[] emailHash = cryptoService.HashEmail(record.Email);

                var existingLeaks = await dbContext.Leaks
                    .Where(l => l.EmailHash == emailHash)
                    .ToListAsync();

                bool foundMatchingLeak = false;

                foreach (var existingLeak in existingLeaks)
                {
                    var passwordHash = cryptoService.HashPassword(record.PlaintextPassword, existingLeak.PasswordSalt);
                    if (passwordHash.SequenceEqual(existingLeak.PasswordHash))
                    {
                        existingLeak.LastSeen = DateTimeOffset.UtcNow;
                        foundMatchingLeak = true;
                        break;
                    }
                }

                if (foundMatchingLeak) continue;

                byte[] salt = cryptoService.GenerateRandomSalt();
                byte[] passwordHashWithSalt = cryptoService.HashPassword(record.PlaintextPassword, salt);
                string domainName = Helper.GetDomainFromEmail(record.Email);

                var domain = dbContext.Domains
                    .Include(d => d.AssociatedByCustomers)
                    .SingleOrDefault(d => d.DomainName == domainName);

                var customers = domain?.AssociatedByCustomers ?? new List<Customer>();

                var newLeak = new Leak
                {
                    Id = Guid.NewGuid(),
                    EmailHash = emailHash,
                    ObfuscatedPassword = Helper.ObfuscatePassword(record.PlaintextPassword),
                    PasswordHash = passwordHashWithSalt,
                    PasswordSalt = salt,
                    PasswordAlgorithmVersion = CryptoService.AlgorithmVersionForPassword,
                    PasswordAlgorithm = CryptoService.AlgorithmForPassword,
                    EMailAlgorithm = CryptoService.AlgorithmForEmail,
                    Domain = domainName,
                    AssociatedCustomers = customers,
                    FirstSeen = DateTimeOffset.UtcNow,
                    LastSeen = DateTimeOffset.UtcNow
                };

                dbContext.Leaks.Add(newLeak); 
            }

            await dbContext.SaveChangesAsync();
        }

        private static IEnumerable<List<T>> Chunk<T>(IEnumerable<T> source, int size)
        {
            List<T> chunk = new(size);
            foreach (var item in source)
            {
                chunk.Add(item);
                if (chunk.Count >= size)
                {
                    yield return chunk;
                    chunk = new(size);
                }
            }
            if (chunk.Any())
                yield return chunk;
        }
    }
}
