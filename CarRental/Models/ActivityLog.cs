using System;

namespace CarRental.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        public string UserName { get; set; } = "";

        public string Action { get; set; } = "";

        public string Description { get; set; } = "";

        public string IpAddress { get; set; } = "";

        public string UserAgent { get; set; } = "";

        public DateTime Timestamp { get; set; }
    }
}
