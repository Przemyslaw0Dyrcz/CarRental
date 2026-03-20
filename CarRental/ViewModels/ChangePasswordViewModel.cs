
using System.ComponentModel.DataAnnotations;
namespace CarRental.ViewModels;
public class ChangePasswordViewModel
{
    [DataType(DataType.Password)]
    public string OldPassword { get; set; }

    [Required(ErrorMessage = "Nowe has³o jest wymagane")]
    [MinLength(8, ErrorMessage = "Minimum 8 znaków")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "Has³o musi zawieraæ du¿¹ literê, ma³¹ literê i cyfrê")]
    public string NewPassword { get; set; }

    [Compare("NewPassword", ErrorMessage = "Has³a nie s¹ zgodne")]
    public string ConfirmNewPassword { get; set; }
}