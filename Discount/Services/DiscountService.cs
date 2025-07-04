using Discount.Models;
using Discount.Repositories;

namespace Discount.Services
{
    public class DiscountService
    {
        private readonly DiscountRepository _repository;
        public DiscountService(DiscountRepository repository)
        {
            _repository = repository;
        }

        public async Task<Models.Discount?> GetBestDiscountAsync(List<string> items, decimal price)
        {
            var discounts = await _repository.GetAllAsync();
            var now = DateTime.UtcNow;
            var applicable = discounts.Where(d => d.ValidDate >= now && d.Items.Any(i => items.Contains(i)));
            Models.Discount? best = null;
            decimal bestValue = 0;
            foreach (var d in applicable)
            {
                decimal value = d.DiscountType == DiscountType.Fix ? d.Value : price * d.Value / 100;
                if (value > bestValue)
                {
                    bestValue = value;
                    best = d;
                }
            }
            return best;
        }
    }
}
