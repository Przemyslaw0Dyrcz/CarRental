using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using CarRental.Data;
using System.Linq;

namespace CarRental.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SecurityController : Controller
    {
        private readonly ApplicationDbContext _db;

        public SecurityController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Dashboard()
        {
            var logins = _db.ActivityLogs.Count(a => a.Action == "LOGIN_SUCCESS");
            var failed = _db.ActivityLogs.Count(a => a.Action == "LOGIN_FAILED");
            var alerts = _db.ActivityLogs.Count(a => a.Action.Contains("BRUTE"));

            ViewBag.Logins = logins;
            ViewBag.Failed = failed;
            ViewBag.Alerts = alerts;

            var logs = _db.ActivityLogs
                .OrderByDescending(x => x.Timestamp)
                .Take(100)
                .ToList();

            return View(logs);
        }
    }
}
