using Discount.Models;
using Discount.Repositories;
using Discount.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add configuration
var configuration = builder.Configuration;
var startupLogger = LoggerFactory.Create(config => 
{
    config.AddConsole();
}).CreateLogger("Program");

// Configure database
try
{
    if (builder.Environment.IsDevelopment())
    {
        // Use in-memory database for development
        startupLogger.LogInformation("Configuring in-memory database for development");
        builder.Services.AddDbContext<DiscountDbContext>(opt => 
            opt.UseInMemoryDatabase("DiscountDb"));
    }
    else
    {
        // Use Cosmos DB for production
        startupLogger.LogInformation("Configuring Cosmos DB for production");
        startupLogger.LogInformation($"Cosmos DB Endpoint: {configuration["CosmosDb:EndpointUri"]}");
        startupLogger.LogInformation($"Cosmos DB Database: {configuration["CosmosDb:DatabaseName"]}");
        
        string endpoint = configuration["CosmosDb:EndpointUri"] ?? "fallback-endpoint";
        string key = configuration["CosmosDb:PrimaryKey"] ?? "fallback-key";
        string dbName = configuration["CosmosDb:DatabaseName"] ?? "DiscountDb";
        
        builder.Services.AddDbContext<DiscountDbContext>(opt => 
            opt.UseCosmos(endpoint, key, dbName));
    }
}
catch (Exception ex)
{
    startupLogger.LogError(ex, "Error configuring database connection");
    // Fall back to in-memory database if Cosmos DB configuration fails
    builder.Services.AddDbContext<DiscountDbContext>(opt => 
        opt.UseInMemoryDatabase("FallbackDb"));
}

// Add services
builder.Services.AddScoped<DiscountRepository>();
builder.Services.AddScoped<DiscountService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // Add production error handling
    app.UseExceptionHandler("/error");
}

app.UseSwagger();
app.UseSwaggerUI();

// Basic health check endpoint
app.MapGet("/health", () => "Application is running");

// Error endpoint
app.MapGet("/error", () => "An error occurred. Please check the application logs.");

// Create Discount
app.MapPost("/api/discount", async (Discount.Models.Discount discount, DiscountRepository repo) =>
{
    await repo.AddAsync(discount);
    return Results.Created($"/api/discount/{discount.Id}", discount);
});

// Update Discount
app.MapPut("/api/discount/{id}", async (string id, Discount.Models.Discount discount, DiscountRepository repo) =>
{
    if (id != discount.Id) return Results.BadRequest();
    await repo.UpdateAsync(discount);
    return Results.Ok(discount);
});

// Delete Discount
app.MapDelete("/api/discount/{id}", async (string id, DiscountRepository repo) =>
{
    await repo.DeleteAsync(id);
    return Results.NoContent();
});

// Apply Discount
app.MapPost("/api/discount/apply", async (List<string> items, decimal price, DiscountService service) =>
{
    var best = await service.GetBestDiscountAsync(items, price);
    if (best == null) return Results.NotFound("No applicable discount.");
    decimal discountValue = best.DiscountType == DiscountType.Fix ? best.Value : price * best.Value / 100;
    return Results.Ok(new { Discount = best, DiscountValue = discountValue, FinalPrice = price - discountValue });
});

// Ensure database is created with error handling
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var dbContextLogger = services.GetRequiredService<ILogger<DiscountDbContext>>();
        
        try
        {
            var dbContext = services.GetRequiredService<DiscountDbContext>();
            if (!app.Environment.IsDevelopment())
            {
                dbContextLogger.LogInformation("Ensuring database is created");
                dbContext.Database.EnsureCreated();
                dbContextLogger.LogInformation("Database created successfully");
            }
        }
        catch (Exception ex)
        {
            dbContextLogger.LogError(ex, "An error occurred while creating the database");
        }
    }
}
catch (Exception ex)
{
    startupLogger.LogError(ex, "An error occurred during app initialization");
}

app.Run();

// Enums for UseType and DiscountType
public enum UseType
{
    SingleUse,
    MultiUse,
    InfinityUse
}

public enum DiscountType
{
    Fix,
    Percentage
}

public partial class Program { }
