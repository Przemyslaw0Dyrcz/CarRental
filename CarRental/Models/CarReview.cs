using System;

namespace CarRental.Models
{
    public class CarReview
    {
        public Guid Id { get; set; }
        public Guid CarId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int Rating { get; set; } // 1-5
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
