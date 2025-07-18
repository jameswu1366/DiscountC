// Discount model with required properties
using System.ComponentModel.DataAnnotations;

namespace Discount.Models
{
    public class Discount
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime ValidDate { get; set; }
        public UseType UseType { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal Value { get; set; }
        public List<string> Items { get; set; } = new List<string>();
    }
}
