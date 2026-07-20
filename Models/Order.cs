using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NehasBeed.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string InvoiceNumber { get; set; } = "";

        // Customer info
        [Required]
        public string CustomerName { get; set; } = "";

        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; } = "";

        public string CustomerPhone { get; set; } = "";
        public string ShippingAddress { get; set; } = "";
        public string City { get; set; } = "";
        public string Postcode { get; set; } = "";
        public string Country { get; set; } = "";

        // Order details
        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal ShippingCost { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        public string PaymentMethod { get; set; } = "Card";

        // Status: Pending, Confirmed, Rejected, Dispatched
        public string Status { get; set; } = "Pending";

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ConfirmedAt { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public List<OrderItem> OrderItems { get; set; } = new();

        // Helper
        public string GetStatusBadgeColor() => Status switch
        {
            "Confirmed"  => "#27ae60",
            "Rejected"   => "#c0392b",
            "Dispatched" => "#3498db",
            "Cancelled"  => "#7f8c8d",
            _            => "#d68910"
        };

        public string GetStatusBg() => Status switch
        {
            "Confirmed"  => "rgba(39,174,96,0.12)",
            "Rejected"   => "rgba(192,57,43,0.12)",
            "Dispatched" => "rgba(52,152,219,0.12)",
            "Cancelled"  => "rgba(127,140,141,0.12)",
            _            => "rgba(243,156,18,0.12)"
        };
    }
}
