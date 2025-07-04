using Discount.Models;
using Microsoft.EntityFrameworkCore;

namespace Discount.Repositories
{
    public class DiscountRepository
    {
        private readonly DiscountDbContext _context;
        public DiscountRepository(DiscountDbContext context)
        {
            _context = context;
        }

        public async Task<Models.Discount?> GetByIdAsync(int id) => await _context.Discounts.FindAsync(id);
        public async Task<List<Models.Discount>> GetAllAsync() => await _context.Discounts.ToListAsync();
        public async Task AddAsync(Models.Discount discount)
        {
            _context.Discounts.Add(discount);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Models.Discount discount)
        {
            _context.Discounts.Update(discount);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount != null)
            {
                _context.Discounts.Remove(discount);
                await _context.SaveChangesAsync();
            }
        }
    }
}
