using CredentialLeakageMonitoring.ApiModels;
using CredentialLeakageMonitoring.Database;
using CredentialLeakageMonitoring.DatabaseModels;
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
        e.Properties.TryGetValue("SourceContext", out LogEventPropertyValue? c) &&
        c.Equals(new ScalarValue("Microsoft.EntityFrameworkCore.Database.Command")))
    .CreateLogger();

builder.Host.UseSerilog();

string? connectionString = builder.Configuration.GetConnectionString("Postgres");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<IngestionService>();
builder.Services.AddScoped<CryptoService>();
builder.Services.AddScoped<QueryService>();
builder.Services.AddScoped<CustomerService>();

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

app.MapGet("/query", async (string email, QueryService queryService) =>
{
    if (string.IsNullOrWhiteSpace(email))
        return Results.BadRequest("Email is required.");
    List<LeakModel> leaks = await queryService.SearchForLeaksByEmail(email);

    return Results.Ok(leaks);
});

app.MapPost("/customers", async (CreateCustomerModel model, CustomerService customerService) =>
{
    if (model == null || string.IsNullOrWhiteSpace(model.Name) || model.AssociatedDomains.Count == 0)
        return Results.BadRequest("Invalid customer data.");

    CustomerModel createdCustomer = await customerService.CreateCustomer(model);
    return Results.Created($"/customers/{createdCustomer.Id}", createdCustomer);
});

app.MapGet("/customers", async (CustomerService customerService) =>
{
    List<CustomerModel> customers = await customerService.GetCustomers();
    return Results.Ok(customers);
});

app.MapGet("/customers/{id:guid}", async (Guid id, CustomerService customerService) =>
{
    CustomerModel? customer = await customerService.GetCustomer(id);
    if (customer == null)
        return Results.NotFound($"Customer with ID {id} not found.");

    return Results.Ok(customer);
});

app.MapPut("/customers/{id:guid}", async (Guid id, CustomerModel model, CustomerService customerService) =>
{
    if (model == null || id != model.Id || string.IsNullOrWhiteSpace(model.Name) || model.AssociatedDomains.Count == 0)
        return Results.BadRequest("Invalid customer data.");

    try
    {
        CustomerModel updatedCustomer = await customerService.UpdateCustomer(model);
        return Results.Ok(updatedCustomer);
    }
    catch (Exception ex)
    {
        return Results.NotFound(ex.Message);
    }
});

app.MapDelete("/customers/{id:guid}", async (Guid id, ApplicationDbContext dbContext) =>
{
    Customer? customer = await dbContext.Customers
        .Include(c => c.AssociatedDomains)
        .SingleOrDefaultAsync(c => c.Id == id);

    if (customer == null)
        return Results.NotFound($"Customer with ID {id} not found.");

    dbContext.Customers.Remove(customer);
    await dbContext.SaveChangesAsync();

    return Results.NoContent();
});

app.MapGet("/customers/{id:guid}/query", async (Guid id, QueryService queryService) =>
{
    List<LeakModel> leaks = await queryService.SearchForLeaksByCustomerId(id);

    return Results.Ok(leaks);
});

app.Run();