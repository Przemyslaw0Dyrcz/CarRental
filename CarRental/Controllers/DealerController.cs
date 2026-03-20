using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CarRental.Controllers
{
    [Authorize(Roles = "Dealer,Admin")]
    public class DealerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DealerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cars = _context.Cars.ToList();
            return View(cars);
        }

        public IActionResult Dashboard()
        {
            var cars = _context.Cars.ToList();
            return View(cars);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(Guid carId, CarStatus status)
        {
            var car = await _context.Cars.FindAsync(carId);

            if (car == null)
                return NotFound();

            var hasActiveReservation = await _context.Reservations.AnyAsync(r =>
                r.CarId == carId &&
                r.Status == ReservationStatus.Active);

            if (status == CarStatus.Available && hasActiveReservation)
            {
                TempData["Error"] = "Cannot set Available - car is currently rented";
                return RedirectToAction(nameof(Edit), new { id = carId });
            }

            if (status == CarStatus.Maintenance && hasActiveReservation)
            {
                TempData["Error"] = "Cannot send to maintenance during active rental";
                return RedirectToAction(nameof(Edit), new { id = carId });
            }

            car.Status = status;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Status updated";

            return RedirectToAction(nameof(Edit), new { id = carId });
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Car car)
        {
            if (!ModelState.IsValid)
                return View(car);

            car.Id = Guid.NewGuid();

            _context.Cars.Add(car);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var car = await _context.Cars.FindAsync(id);

            if (car == null)
                return NotFound();

            return View(car);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Car updatedCar)
        {
            var car = await _context.Cars.FindAsync(updatedCar.Id);

            if (car == null)
                return NotFound();

            if (!ModelState.IsValid)
                return View(updatedCar);

            car.Brand = updatedCar.Brand;
            car.Model = updatedCar.Model;
            car.Year = updatedCar.Year;
            car.PricePerDay = updatedCar.PricePerDay;
            car.Seats = updatedCar.Seats;
            car.EngineCapacity = updatedCar.EngineCapacity;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Car updated";

            return RedirectToAction(nameof(Edit), new { id = car.Id });
        }

        public IActionResult Delete(Guid id)
        {
            var car = _context.Cars.FirstOrDefault(c => c.Id == id);

            if (car != null)
            {
                _context.Cars.Remove(car);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Dealer,Admin")]
        public async Task<IActionResult> DealerReservations()
        {
            var reservations = await _context.Reservations.ToListAsync();
            return View(reservations);
        }
    }
}