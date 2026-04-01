using CarRental.Data;
using CarRental.Models;
using CarRental.Services.Interface;
using CarRental.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CarRental.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAdminService _adminService;
        private readonly IActivityLogger _logger;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IAdminService adminService,
            IActivityLogger logger)
        {
            _userManager = userManager;
            _adminService = adminService;
            _logger = logger;
        }

        private string? GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();

            var model = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                model.Add(new UserRoleViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    Role = roles.FirstOrDefault() ?? "User",
                    LockoutEnd = user.LockoutEnd
                });
            }

            model = model
                .OrderBy(u => u.Role == "Admin" ? 0 :
                              u.Role == "Dealer" ? 1 : 2)
                .ThenBy(u => u.Email)
                .ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(string userId, string role)
        {
            var adminId = GetUserId();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            var admins = await _userManager.GetUsersInRoleAsync("Admin");

            if (currentRoles.Contains("Admin") && admins.Count == 1 && role != "Admin")
            {
                TempData["msg"] = "Cannot remove the last admin.";
                return RedirectToAction(nameof(Index));
            }

            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, role);

            await _logger.LogAsync(
                adminId ?? "unknown",
                "ADMIN_ROLE_CHANGED",
                $"Changed role of {user.Email} to {role}"
            );

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUser(string userId)
        {
            var adminId = GetUserId();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            await _userManager.SetLockoutEndDateAsync(
                user,
                DateTimeOffset.UtcNow.AddYears(100)
            );

            await _logger.LogAsync(
                adminId ?? "unknown",
                "ADMIN_LOCK_USER",
                $"Locked user {user.Email}"
            );

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlockUser(string userId)
        {
            var adminId = GetUserId();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            await _userManager.SetLockoutEndDateAsync(user, null);

            await _logger.LogAsync(
                adminId ?? "unknown",
                "ADMIN_UNLOCK_USER",
                $"Unlocked user {user.Email}"
            );

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string userId)
        {
            var adminId = GetUserId();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
            if (user.Id == adminId)
    throw new InvalidOperationException("Cannot delete yourself");
            await _userManager.DeleteAsync(user);

            await _logger.LogAsync(
                adminId ?? "unknown",
                "ADMIN_DELETE_USER",
                $"Deleted user {user.Email}"
            );

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string userId, string newPassword)
        {
            var adminId = GetUserId();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _userManager.ResetPasswordAsync(user, token, newPassword);

            await _logger.LogAsync(
                adminId ?? "unknown",
                "ADMIN_RESET_PASSWORD",
                $"Reset password for {user.Email}"
            );

            return RedirectToAction(nameof(Index));
        }

        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(RegisterViewModel model)
        {
            var adminId = GetUserId();

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
                await _userManager.AddToRoleAsync(user, "User");

                await _logger.LogAsync(
                    adminId ?? "unknown",
                    "ADMIN_CREATE_USER",
                    $"Created user {user.Email}"
                );

                return RedirectToAction(nameof(Index));
            }

            foreach (var e in result.Errors)
                ModelState.AddModelError("", e.Description);

            return View(model);
        }

        public async Task<IActionResult> Logs()
        {
            var logs = await _adminService.GetLogsAsync();
            return View(logs);
        }

        public async Task<IActionResult> SecurityDashboard()
        {
            var model = await _adminService.GetSecurityDashboardAsync();
            return View(model);
        }

        public async Task<IActionResult> Alerts()
        {
            var alerts = await _adminService.GetAlertsAsync();
            return View(alerts);
        }

        public async Task<IActionResult> UserSettings()
        {
            var model = await _adminService.GetSettingsAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserSettings(SecuritySettingsViewModel model)
        {
            var adminId = GetUserId();

            if (!ModelState.IsValid)
                return View(model);

            await _adminService.UpdateSettingsAsync(model);

            await _logger.LogAsync(
                adminId ?? "unknown",
                "SECURITY_SETTINGS_CHANGED",
                "Admin updated security configuration"
            );

            TempData["msg"] = "Settings updated";

            return RedirectToAction(nameof(UserSettings));
        }
    }
}