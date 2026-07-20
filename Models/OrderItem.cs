using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NehasBeed.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Required]
        public string ProductName { get; set; } = "";

        [Column(TypeName = "decimal(8,2)")]
        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; } = 1;

        public string? SelectedColor { get; set; }

        public string? ProductImage { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal LineTotal => UnitPrice * Quantity;
    }
}
