
using CarRental.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using CarRental.Models;

namespace CarRental.Controllers
{
    [Route("api/availability")]
    [ApiController]
    public class AvailabilityController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public AvailabilityController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> IsAvailableAsync(Guid carId, DateTime start, DateTime end)
        {
            if (start >= end)
                throw new ArgumentException("Invalid date range");

            var now = DateTime.UtcNow;

            return !await _db.Reservations.AnyAsync(r =>
                r.CarId == carId &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Active) &&
                r.ToDate > now &&
                r.FromDate < end &&
                r.ToDate > start
            );
        }
    }
}
