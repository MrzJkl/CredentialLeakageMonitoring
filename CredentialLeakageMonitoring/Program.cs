using CredentialLeakageMonitoring.Database;
using CredentialLeakageMonitoring.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .Filter.ByExcluding(e =>
        e.Level == LogEventLevel.Information &&
        e.Properties.TryGetValue("SourceContext", out var c) &&
        c.Equals(new ScalarValue("Microsoft.EntityFrameworkCore.Database.Command")))
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddLogging(options =>
{
    options.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning); // SQL unterdrücken
});

string? connectionString = builder.Configuration.GetConnectionString("Postgres");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<IngestionService>();
builder.Services.AddScoped<CryptoService>();

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapPost("/upload", async (IFormFile file, IngestionService ingestionService) =>
{
    if (file is null || file.Length == 0)
        return Results.BadRequest("No file uploaded.");

    await ingestionService.IngestCsvAsync(file.OpenReadStream());

    return Results.Ok();
}).DisableAntiforgery();

app.Run();