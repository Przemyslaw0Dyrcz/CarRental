using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using CarRental.Data;
using CarRental.Models;

namespace CarRental.Services
{
    public class CarAvailabilityService
    {
        private readonly ApplicationDbContext _db;

        public CarAvailabilityService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> IsAvailableAsync(Guid carId, DateTime start, DateTime end)
        {
            if (start >= end)
                throw new ArgumentException("Invalid date range");

            return !await _db.Reservations.AnyAsync(r =>
                r.CarId == carId &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Active) &&
                r.FromDate < end &&
                r.ToDate > start);
        }
    }
}