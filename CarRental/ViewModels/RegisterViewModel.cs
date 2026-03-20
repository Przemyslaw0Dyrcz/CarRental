using System.ComponentModel.DataAnnotations;

namespace CarRental.ViewModels;
public class RegisterViewModel
{
    [Required(ErrorMessage = "Nazwa u¿ytkownika jest wymagana")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Email jest wymagany")]
    [EmailAddress(ErrorMessage = "Niepoprawny format emaila")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Has³o jest wymagane")]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "Has³o musi mieæ minimum 8 znaków")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "Has³o musi zawieraæ du¿¹ literê, ma³¹ literê i cyfrê")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Potwierdzenie has³a jest wymagane")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Has³a nie s¹ takie same")]
    public string ConfirmPassword { get; set; }

    [Required(ErrorMessage = "Imiê i nazwisko jest wymagane")]
    public string FullName { get; set; }
}