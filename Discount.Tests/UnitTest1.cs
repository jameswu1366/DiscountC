using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Discount;
using Discount.Models;
using System.Collections.Generic;
using System.Text.Json;

namespace Discount.Tests
{
    public class DiscountApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public DiscountApiTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateDiscount_ReturnsCreated()
        {
            var discount = new Discount.Models.Discount
            {
                Id = 1,
                ValidDate = System.DateTime.UtcNow.AddDays(1),
                UseType = UseType.SingleUse,
                DiscountType = DiscountType.Fix,
                Value = 10,
                Items = new List<string> { "item1" }
            };
            var response = await _client.PostAsJsonAsync("/api/discount", discount);
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task UpdateDiscount_ReturnsOk()
        {
            var discount = new Discount.Models.Discount
            {
                Id = 2,
                ValidDate = System.DateTime.UtcNow.AddDays(1),
                UseType = UseType.MultiUse,
                DiscountType = DiscountType.Percentage,
                Value = 20,
                Items = new List<string> { "item2" }
            };
            await _client.PostAsJsonAsync("/api/discount", discount);
            discount.Value = 25;
            var response = await _client.PutAsJsonAsync($"/api/discount/{discount.Id}", discount);
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task DeleteDiscount_ReturnsNoContent()
        {
            var discount = new Discount.Models.Discount
            {
                Id = 3,
                ValidDate = System.DateTime.UtcNow.AddDays(1),
                UseType = UseType.InfinityUse,
                DiscountType = DiscountType.Fix,
                Value = 5,
                Items = new List<string> { "item3" }
            };
            await _client.PostAsJsonAsync("/api/discount", discount);
            var response = await _client.DeleteAsync($"/api/discount/{discount.Id}");
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task ApplyDiscount_ReturnsBestDeal()
        {
            var discount1 = new Discount.Models.Discount
            {
                Id = 4,
                ValidDate = System.DateTime.UtcNow.AddDays(1),
                UseType = UseType.MultiUse,
                DiscountType = DiscountType.Fix,
                Value = 10,
                Items = new List<string> { "item4" }
            };
            var discount2 = new Discount.Models.Discount
            {
                Id = 5,
                ValidDate = System.DateTime.UtcNow.AddDays(1),
                UseType = UseType.MultiUse,
                DiscountType = DiscountType.Percentage,
                Value = 50,
                Items = new List<string> { "item4" }
            };
            await _client.PostAsJsonAsync("/api/discount", discount1);
            await _client.PostAsJsonAsync("/api/discount", discount2);
            var items = new List<string> { "item4" };
            var response = await _client.PostAsJsonAsync($"/api/discount/apply?items=item4&price=100", items);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (!root.TryGetProperty("discountValue", out var discountValueProp))
                {
                    Assert.Fail($"discountValue property not found. Response: {json}");
                }
                var discountValue = discountValueProp.GetDecimal();
                Assert.Equal(50, discountValue);
            }
            catch (JsonException)
            {
                Assert.False(true, $"Invalid JSON response: {json}");
            }
        }
    }
}