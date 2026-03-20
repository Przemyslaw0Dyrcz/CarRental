using Microsoft.AspNetCore.Identity;

namespace CarRental.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }

        public bool IsBlocked { get; set; }

        public bool ForceChangePassword { get; set; }

        public DateTime? PasswordChangedDate { get; set; }

        public int PasswordValidityDays { get; set; } = 90;
    }
}
