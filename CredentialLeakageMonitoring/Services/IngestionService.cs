using CredentialLeakageMonitoring.ApiModels;
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using CredentialLeakageMonitoring.DatabaseModels;
using CredentialLeakageMonitoring.Database;

namespace CredentialLeakageMonitoring.Services
{
    public class IngestionService(ApplicationDbContext dbContext)
    {
        public static async Task<List<Leak>> IngestCsvAsync(Stream csvStream)
        {
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = false,
                TrimOptions = TrimOptions.Trim,
            });

            var records = csv.GetRecordsAsync<IngestionLeakModel>().ConfigureAwait(false);

            await foreach (var record in records)
            {

            }

            return [];
        }
    }
}
