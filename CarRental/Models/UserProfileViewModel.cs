namespace CarRental.Models
{
    public class UserProfileViewModel
    {
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Roles { get; set; } = "";
        public string LockoutEnd { get; set; } = "";
        public bool PasswordChanged { get; set; }
    }
}
