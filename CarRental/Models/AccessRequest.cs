using System;

namespace CarRental.Models
{
    public class AccessRequest
    {
        public int Id { get; set; }
        public string RequesterId { get; set; } = string.Empty;
        public string RequestedRole { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedById { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string Status { get; set; } = "Pending"; 
        public string? Reason { get; set; }
    }
}
