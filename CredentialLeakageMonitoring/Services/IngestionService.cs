using CredentialLeakageMonitoring.ApiModels;
using CredentialLeakageMonitoring.Database;
using CredentialLeakageMonitoring.DatabaseModels;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CredentialLeakageMonitoring.Services
{
    public class IngestionService(ApplicationDbContext dbContext, CryptoService cryptoService, ILogger<IngestionService> log)
    {
        public async Task IngestCsvAsync(Stream csvStream)
        {
            using StreamReader reader = new(csvStream);
            using CsvReader csv = new(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = false,
                TrimOptions = TrimOptions.Trim,
            });

            System.Runtime.CompilerServices.ConfiguredCancelableAsyncEnumerable<IngestionLeakModel> records = csv.GetRecordsAsync<IngestionLeakModel>().ConfigureAwait(false);
            List<Leak> newLeaks = [];

            await foreach (IngestionLeakModel? record in records)
            {
                byte[] emailHash = cryptoService.HashEmail(record.Email);

                List<Leak> existingLeaksForMailaddress = await dbContext.Leaks
                    .Where(l => l.EmailHash == emailHash)
                    .ToListAsync()
                    .ConfigureAwait(false);

                log.LogInformation("Found {Count} existing leaks for email {Email}", existingLeaksForMailaddress.Count, record.Email);

                bool foundMatchingLeak = false;

                foreach (Leak? existingLeak in existingLeaksForMailaddress)
                {
                    if (foundMatchingLeak)
                    {
                        break;
                    }

                    byte[] passwordHashWithExistingSalt = cryptoService.HashPassword(record.PlaintextPassword, existingLeak.PasswordSalt);

                    if (passwordHashWithExistingSalt.SequenceEqual(existingLeak.PasswordHash))
                    {
                        log.LogInformation("Found matching leak for email {Email} with password {Password}", record.Email, existingLeak.ObfuscatedPassword);
                        existingLeak.LastSeen = DateTimeOffset.UtcNow;
                        await dbContext.SaveChangesAsync().ConfigureAwait(false);
                        foundMatchingLeak = true;

                        continue;
                    }
                }

                if (foundMatchingLeak)
                {
                    // Next record
                    continue;
                }

                log.LogInformation("No matching leak found for email {Email}. Adding leak as new.", record.Email);

                // Leak is really new
                byte[] salt = cryptoService.GenerateRandomSalt();
                byte[] passwordHashWithRandomSalt = cryptoService.HashPassword(record.PlaintextPassword, salt);
                string domainName = Helper.GetDomainFromEmail(record.Email);

                Domain? domain = dbContext.Domains
                    .Include(d => d.AssociatedByCustomers)
                    .SingleOrDefault(d => d.DomainName == domainName);
                List<Customer> customers = domain?.AssociatedByCustomers ?? [];

                Leak newLeak = new()
                {
                    Id = Guid.NewGuid(),
                    EmailHash = emailHash,
                    ObfuscatedPassword = Helper.ObfuscatePassword(record.PlaintextPassword),
                    PasswordHash = passwordHashWithRandomSalt,
                    PasswordSalt = salt,
                    PasswordAlgorithmVersion = CryptoService.AlgorithmVersionForPassword,
                    PasswordAlgorithm = CryptoService.AlgorithmForPassword,
                    EMailAlgorithm = CryptoService.AlgorithmForEmail,
                    Domain = domainName,
                    AssociatedCustomers = customers,
                    FirstSeen = DateTimeOffset.UtcNow,
                    LastSeen = DateTimeOffset.UtcNow
                };

                newLeaks.Add(newLeak);

                dbContext.Leaks.Add(newLeak);
                await dbContext.SaveChangesAsync().ConfigureAwait(false);
            }

        }
    }
}
