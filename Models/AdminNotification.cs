using System;
using System.ComponentModel.DataAnnotations;

namespace NehasBeed.Models
{
    public class AdminNotification
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = "";

        // "info" | "warning" | "success"
        public string Type { get; set; } = "info";

        public bool IsActive { get; set; } = true;

        [MaxLength(300)]
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
