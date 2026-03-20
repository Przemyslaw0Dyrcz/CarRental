using CarRental.Data;
using CarRental.Middleware;
using CarRental.Models;
using CarRental.Services;
using CarRental.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CarRental.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        private string? GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Index", "Cars");

            return View(user);
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Index", "Cars");

            await ActivityLogger.LogAsync(
                _context,
                HttpContext,
                user.Id,
                "ACCOUNT_DELETED",
                $"User {user.Email} deleted account"
            );

            await _userManager.DeleteAsync(user);
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Cars");
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var result = await _signInManager.PasswordSignInAsync(
                model.Username,
                model.Password,
                false,
                true);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(model.Username);

                BruteForceDetectionMiddleware.ResetAttempts(ip);

                await ActivityLogger.LogAsync(
                    _context,
                    HttpContext,
                    user?.Id,
                    "LOGIN_SUCCESS",
                    $"User logged in from {ip}"
                );

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Cars");
            }

            if (result.IsLockedOut)
            {
                await ActivityLogger.LogAsync(
                    _context,
                    HttpContext,
                    model.Username,
                    "ACCOUNT_LOCKED",
                    $"Account locked due to failed attempts from {ip}"
                );

                ModelState.AddModelError("", "Account is locked.");
                return View(model);
            }

            BruteForceDetectionMiddleware.RegisterFailedAttempt(ip);

            await ActivityLogger.LogAsync(
                _context,
                HttpContext,
                model.Username,
                "LOGIN_FAILED",
                $"Invalid login attempt from {ip}"
            );

            ModelState.AddModelError("Password", "Nieprawid³owe has³o");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                if (!await _userManager.IsInRoleAsync(user, "User"))
                {
                    await _userManager.AddToRoleAsync(user, "User");
                }

                await _signInManager.SignInAsync(user, false);

                await ActivityLogger.LogAsync(
                    _context,
                    HttpContext,
                    user.Id,
                    "USER_REGISTERED",
                    $"New user registered: {user.Email}"
                );

                return RedirectToAction("Index", "Cars");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userId = GetUserId();

            await ActivityLogger.LogAsync(
                _context,
                HttpContext,
                userId,
                "LOGOUT",
                "User logged out"
            );

            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Cars");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            IdentityResult result;

            if (string.IsNullOrEmpty(model.OldPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                result = await _userManager.ResetPasswordAsync(
                    user,
                    token,
                    model.NewPassword
                );
            }
            else
            {
                result = await _userManager.ChangePasswordAsync(
                    user,
                    model.OldPassword,
                    model.NewPassword
                );
            }

            if (result.Succeeded)
            {
                await ActivityLogger.LogAsync(
                    _context,
                    HttpContext,
                    user.Id,
                    "PASSWORD_CHANGED",
                    "User changed password"
                );

                await _signInManager.RefreshSignInAsync(user);

                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }
    }
}