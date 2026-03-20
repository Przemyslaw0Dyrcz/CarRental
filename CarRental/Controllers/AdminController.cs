using CarRental.Data;
using CarRental.Models;
using CarRental.Services;
using CarRental.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CarRental.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
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

            await ActivityLogger.LogAsync(
                _context,
                HttpContext,
                adminId,
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

            await ActivityLogger.LogAsync(
                _context,
                HttpContext,
                adminId,
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

            await ActivityLogger.LogAsync(
                _context,
                HttpContext,
                adminId,
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

            await _userManager.DeleteAsync(user);

            await ActivityLogger.LogAsync(
                _context,
                HttpContext,
                adminId,
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

            await ActivityLogger.LogAsync(
                _context,
                HttpContext,
                adminId,
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

                await ActivityLogger.LogAsync(
                    _context,
                    HttpContext,
                    adminId,
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
            var logs = await _context.ActivityLogs
                .OrderByDescending(x => x.Timestamp)
                .Take(200)
                .ToListAsync();

            return View(logs);
        }

        public async Task<IActionResult> SecurityDashboard()
        {
            var failedLogins = await _context.ActivityLogs
                .Where(x => x.Action == "LOGIN_FAILED")
                .ToListAsync();

            var alerts = await _context.ActivityLogs
                .Where(x =>
                    x.Action.Contains("BRUTE_FORCE") ||
                    x.Action.Contains("LOCK") ||
                    x.Action.Contains("ALERT"))
                .ToListAsync();

            var recent = await _context.ActivityLogs
                .OrderByDescending(x => x.Timestamp)
                .Take(20)
                .ToListAsync();

            var model = new SecurityDashboardViewModel
            {
                FailedLogins = failedLogins.Count,
                Alerts = alerts.Count,
                Users = await _context.Users.CountAsync(),
                Reservations = await _context.Reservations.CountAsync(),

                FailedLoginEvents = failedLogins.Take(20).ToList(),
                AlertEvents = alerts.Take(20).ToList(),
                RecentEvents = recent
            };

            return View(model);
        }

        public IActionResult Alerts()
        {
            var alerts = _context.ActivityLogs
                .Where(x =>
                    x.Action.Contains("BRUTE_FORCE") ||
                    x.Action.Contains("LOCK") ||
                    x.Action.Contains("ALERT"))
                .OrderByDescending(x => x.Timestamp)
                .Take(200)
                .ToList();

            return View(alerts);
        }

        public async Task<IActionResult> UserSettings()
        {
            var settings = await _context.SecuritySettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new SecuritySettings();
                _context.SecuritySettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            var model = new SecuritySettingsViewModel
            {
                EnableBruteForceProtection = settings.EnableBruteForceProtection,
                MaxLoginAttempts = settings.MaxLoginAttempts,
                SessionTimeoutMinutes = settings.SessionTimeoutMinutes,
                LockoutMinutes = settings.LockoutMinutes
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserSettings(SecuritySettingsViewModel model)
        {
            var adminId = GetUserId();

            if (!ModelState.IsValid)
                return View(model);

            var settings = await _context.SecuritySettings.FirstAsync();

            settings.EnableBruteForceProtection = model.EnableBruteForceProtection;
            settings.MaxLoginAttempts = model.MaxLoginAttempts;
            settings.SessionTimeoutMinutes = model.SessionTimeoutMinutes;
            settings.LockoutMinutes = model.LockoutMinutes;

            await _context.SaveChangesAsync();

            await ActivityLogger.LogAsync(
                _context,
                HttpContext,
                adminId,
                "SECURITY_SETTINGS_CHANGED",
                "Admin updated security configuration"
            );

            TempData["msg"] = "Settings updated";

            return RedirectToAction(nameof(UserSettings));
        }
    }
}