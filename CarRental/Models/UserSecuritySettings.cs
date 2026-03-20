
using System;

namespace CarRental.Models
{
    public class UserSecuritySettings
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public int FailedLoginLimit { get; set; } = 5;
        public int SessionTimeoutMinutes { get; set; } = 15;
        public bool IsLocked { get; set; } = false;
        public int LockoutDurationMinutes { get; set; } = 15; 
    }
}
