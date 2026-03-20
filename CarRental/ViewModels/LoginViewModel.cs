using System.ComponentModel.DataAnnotations;


namespace CarRental.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Nazwa u¿ytkownika jest wymagana")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Has³o jest wymagane")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
