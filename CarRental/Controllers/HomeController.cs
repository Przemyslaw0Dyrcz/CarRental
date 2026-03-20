using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using CarRental.Models;

namespace CarRental.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Cars");
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            var roles = await _userManager.GetRolesAsync(user);
            var model = new UserProfileViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName ?? "(brak)",
                Roles = string.Join(", ", roles),
                LockoutEnd = user.LockoutEnd?.ToString("yyyy-MM-dd HH:mm") ?? "aktywny"
            };
            return View(model);
        }
    }
}
