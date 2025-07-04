using Microsoft.EntityFrameworkCore;
using Discount.Models;

namespace Discount.Repositories
{
    public class DiscountDbContext : DbContext
    {
        public DiscountDbContext(DbContextOptions<DiscountDbContext> options) : base(options) { }
        public DbSet<Discount.Models.Discount> Discounts { get; set; }
    }
}
