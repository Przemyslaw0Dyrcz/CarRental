namespace CarRental.ViewModels
{
    public class SecuritySettingsViewModel
    {
        public bool EnableBruteForceProtection { get; set; }

        public int MaxLoginAttempts { get; set; }

        public int IpWindowMinutes { get; set; }

        public int AccountLockoutAttempts { get; set; }

        public int LockoutMinutes { get; set; }

        public int SessionTimeoutMinutes { get; set; }
    }
}