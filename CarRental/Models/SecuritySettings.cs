namespace CarRental.Models
{
    public class SecuritySettings
    {
        public int Id { get; set; }

        public bool EnableBruteForceProtection { get; set; } = true;

        public int MaxLoginAttempts { get; set; } = 10;

        public int IpWindowMinutes { get; set; } = 15;

        public int AccountLockoutAttempts { get; set; } = 5;

        public int LockoutMinutes { get; set; } = 15;

        public int SessionTimeoutMinutes { get; set; } = 15;
    }
}