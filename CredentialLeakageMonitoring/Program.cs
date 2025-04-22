using CredentialLeakageMonitoring.Database;
using CredentialLeakageMonitoring.Services;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("Postgres");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<IngestionService>();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/upload", async (IFormFile file, IngestionService ingestionService) =>
{
    if (file is null || file.Length == 0)
        return Results.BadRequest("No file uploaded.");

    return Results.Ok();
});

app.Run();