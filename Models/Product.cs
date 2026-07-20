using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NehasBeed.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = "";

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = "";

        [Required]
        [Column(TypeName = "decimal(8,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? OriginalPrice { get; set; }

        public string? Description { get; set; }

        // Stored as JSON string
        public string? Colors { get; set; }

        [MaxLength(100)]
        public string? Badge { get; set; }

        [MaxLength(300)]
        public string? Image { get; set; }

        // Stored as JSON string
        public string? Images { get; set; }

        public bool InStock { get; set; } = true;

        // Tracks available inventory; 0 = sold out
        public int StockQuantity { get; set; } = 0;

        [Column(TypeName = "decimal(3,1)")]
        public decimal Rating { get; set; } = 0;

        public int Reviews { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Helper methods
        public string[] GetColors()
        {
            if (string.IsNullOrEmpty(Colors)) return Array.Empty<string>();
            try { return System.Text.Json.JsonSerializer.Deserialize<string[]>(Colors) ?? Array.Empty<string>(); }
            catch { return Array.Empty<string>(); }
        }

        public string[] GetImages()
        {
            if (string.IsNullOrEmpty(Images)) return Array.Empty<string>();
            try { return System.Text.Json.JsonSerializer.Deserialize<string[]>(Images) ?? Array.Empty<string>(); }
            catch { return Array.Empty<string>(); }
        }

        public string GetStars()
        {
            int full = (int)Math.Floor(Rating);
            bool half = (Rating - full) >= 0.5m;
            return new string('★', full) + (half ? "½" : "");
        }
    }
}
