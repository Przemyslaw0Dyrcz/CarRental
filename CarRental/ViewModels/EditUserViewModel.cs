using System.ComponentModel.DataAnnotations;

namespace CarRental.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? FullName { get; set; }

        public bool IsBlocked { get; set; }

        [Display(Name = "Password validity (days, blank = default)")]
        public int? PasswordValidityDays { get; set; }

        [Display(Name = "Set new password (optional)")]
        public string? SetNewPassword { get; set; }

        [Display(Name = "Force change on next login")]
        public bool ForceChangeOnNextLogin { get; set; }
    }
}
