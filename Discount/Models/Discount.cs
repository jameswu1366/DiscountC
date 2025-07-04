// Discount model with required properties
namespace Discount.Models
{
    public class Discount
    {
        public int Id { get; set; }
        public DateTime ValidDate { get; set; }
        public UseType UseType { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal Value { get; set; }
        public List<string> Items { get; set; } = new List<string>();
    }
}
