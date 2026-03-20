namespace CarRental.ViewModels
{
    public class UserRoleViewModel
    {
        public string Id { get; set; }

        public string Email { get; set; }

        public string UserName { get; set; }

        public string Role { get; set; }

        public DateTimeOffset? LockoutEnd { get; set; }
    }
}