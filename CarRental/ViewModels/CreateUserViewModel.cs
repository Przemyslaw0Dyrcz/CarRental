using System.ComponentModel.DataAnnotations;

namespace CarRental.ViewModels
{
    public class CreateUserViewModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? FullName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string TemporaryPassword { get; set; } = string.Empty;
    }
}
