using Discount.Models;
using Discount.Repositories;
using Discount.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDbContext<DiscountDbContext>(opt => opt.UseInMemoryDatabase("DiscountDb"));
builder.Services.AddScoped<DiscountRepository>();
builder.Services.AddScoped<DiscountService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Create Discount
app.MapPost("/api/discount", async (Discount.Models.Discount discount, DiscountRepository repo) =>
{
    await repo.AddAsync(discount);
    return Results.Created($"/api/discount/{discount.Id}", discount);
});

// Update Discount
app.MapPut("/api/discount/{id}", async (int id, Discount.Models.Discount discount, DiscountRepository repo) =>
{
    if (id != discount.Id) return Results.BadRequest();
    await repo.UpdateAsync(discount);
    return Results.Ok(discount);
});

// Delete Discount
app.MapDelete("/api/discount/{id}", async (int id, DiscountRepository repo) =>
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
