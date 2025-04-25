using CredentialLeakageMonitoring.API.ApiModels;
using CredentialLeakageMonitoring.API.Database;
using CredentialLeakageMonitoring.API.DatabaseModels;
using CredentialLeakageMonitoring.API.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Credential Leakage Monitoring API",
        Version = "v1",
        Description = "An API to monitor and manage credential leakage data.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Moritz Jökel",
            Email = "moritz.joekel.2022@leibniz-fh.de",
        },

    });
});
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Async(c => c.Console())
    .Filter.ByExcluding(e =>
        e.Level == LogEventLevel.Information &&
        e.Properties.TryGetValue("SourceContext", out LogEventPropertyValue? c) &&
        c.Equals(new ScalarValue("Microsoft.EntityFrameworkCore.Database.Command")))
    .CreateLogger();

builder.Host.UseSerilog();

string? connectionString = builder.Configuration.GetConnectionString("Postgres");
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<IngestionService>();
builder.Services.AddScoped<CryptoService>();
builder.Services.AddScoped<QueryService>();
builder.Services.AddScoped<CustomerService>();

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod());

app.UseHttpsRedirection();

app.MapPost("/ingest", async (IFormFile file, IngestionService ingestionService) =>
{
    if (file is null || file.Length == 0)
        return Results.BadRequest("No file uploaded.");

    await ingestionService.IngestCsvAsync(file.OpenReadStream());

    return Results.NoContent();
})
.DisableAntiforgery()
.WithDescription("Upload a CSV file containing newly leaked credentials. CSV should be separated with colon. E.g. someone@example.com,mysecretpassword")
.Produces(StatusCodes.Status204NoContent);

app.MapGet("/query", async (string email, QueryService queryService) =>
{
    if (string.IsNullOrWhiteSpace(email))
        return Results.BadRequest("Email is required.");
    List<LeakModel> leaks = await queryService.SearchForLeaksByEmail(email);

    return Results.Ok(leaks);
})
.WithDescription("Query the credentials database for your email address to see if its leaked.")
.Produces<List<LeakModel>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest);

app.MapPost("/customers", async (CreateCustomerModel model, CustomerService customerService) =>
{
    if (model == null || string.IsNullOrWhiteSpace(model.Name) || model.AssociatedDomains.Count == 0)
        return Results.BadRequest("Invalid customer data.");

    CustomerModel createdCustomer = await customerService.CreateCustomer(model);
    return Results.Created($"/customers/{createdCustomer.Id}", createdCustomer);
})
.WithDescription("Create a new customer.")
.Produces<CustomerModel>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest);

app.MapGet("/customers", async (CustomerService customerService) =>
{
    List<CustomerModel> customers = await customerService.GetCustomers();
    return Results.Ok(customers);
})
.WithDescription("List all existing customers.")
.Produces<List<CustomerModel>>(StatusCodes.Status200OK);

app.MapGet("/customers/{id:guid}", async (Guid id, CustomerService customerService) =>
{
    CustomerModel? customer = await customerService.GetCustomer(id);
    if (customer == null)
        return Results.NotFound($"Customer with ID {id} not found.");

    return Results.Ok(customer);
})
.WithDescription("Get a single customer.")
.Produces<CustomerModel>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

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
})
.WithDescription("Update an existing customer.")
.Produces<CustomerModel>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status404NotFound);

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
})
.WithDescription("Delete an existing customer.")
.Produces<CustomerModel>(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);

app.MapGet("/customers/{id:guid}/query", async (Guid id, QueryService queryService) =>
{
    List<LeakModel> leaks = await queryService.SearchForLeaksByCustomerId(id);

    return Results.Ok(leaks);
})
.WithDescription("Query the database to search for existing leaks for a possible newly created customer.")
.Produces<List<LeakModel>>(StatusCodes.Status200OK);

app.Run();