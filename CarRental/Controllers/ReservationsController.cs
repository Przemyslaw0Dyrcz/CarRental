using CarRental.Data;
using CarRental.Models;
using CarRental.Services;
using CarRental.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CarRental.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly IReservationService _service;
        private readonly ApplicationDbContext _context;

        public ReservationsController(IReservationService service, ApplicationDbContext context)
        {
            _service = service;
            _context = context;
        }

        private string? GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> Create(Guid carId)
        {
            var car = await _context.Cars.FindAsync(carId);

            if (car == null)
                return NotFound();

            var vm = new CreateReservationViewModel
            {
                CarId = car.Id,
                Brand = car.Brand,
                Model = car.Model,
                Year = car.Year,
                PricePerDay = car.PricePerDay,
                Seats = car.Seats,
                EngineCapacity = car.EngineCapacity,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1)
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReservationViewModel vm)
        {
            var userId = GetUserId();
            var car = await _context.Cars.FindAsync(vm.CarId);

            if (car == null)
                return NotFound();

            if (!vm.StartDate.HasValue || !vm.EndDate.HasValue)
            {
                ModelState.AddModelError("", "Select dates");
            }
            else
            {
                var start = vm.StartDate.Value;
                var end = vm.EndDate.Value;

                if (start >= end)
                    ModelState.AddModelError("", "Invalid date range");

                if (start < DateTime.Today)
                    ModelState.AddModelError("", "Date cannot be in the past");

                if ((end - start).Days < 1)
                    ModelState.AddModelError("", "Minimum 1 day");

                var overlap = await _context.Reservations.AnyAsync(r =>
                    r.CarId == vm.CarId &&
                    r.Status != ReservationStatus.Cancelled &&
                    !(end <= r.FromDate || start >= r.ToDate)
                );

                if (overlap)
                    ModelState.AddModelError("", "Car is already reserved in this period");
            }

            if (!ModelState.IsValid)
            {
                vm.Brand = car.Brand;
                vm.Model = car.Model;
                vm.Year = car.Year;
                vm.PricePerDay = car.PricePerDay;
                vm.Seats = car.Seats;
                vm.EngineCapacity = car.EngineCapacity;

                return View(vm);
            }

            var days = (vm.EndDate!.Value - vm.StartDate!.Value).Days;

            var reservation = new Reservation
            {
                CarId = vm.CarId,
                UserId = userId!,
                FromDate = vm.StartDate.Value,
                ToDate = vm.EndDate.Value,
                TotalPrice = days * car.PricePerDay,
                Status = ReservationStatus.Pending
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            await ActivityLogger.LogAsync(
                _context,
                HttpContext,
                userId,
                "RESERVATION_CREATED_BY_USER",
                $"CarId={vm.CarId}, From={vm.StartDate}, To={vm.EndDate}"
            );

            return RedirectToAction("My");
        }

        public async Task<IActionResult> My()
        {
            var userId = GetUserId();

            var reservations = await _context.Reservations
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.FromDate)
                .Select(r => new MyReservationViewModel
                {
                    Id = r.Id,
                    FromDate = r.FromDate,
                    ToDate = r.ToDate,
                    TotalPrice = r.TotalPrice,
                    Status = r.Status,
                    PaymentStatus = r.PaymentStatus,
                    CarName = _context.Cars
                        .Where(c => c.Id == r.CarId)
                        .Select(c => c.Brand + " " + c.Model)
                        .FirstOrDefault() ?? "Unknown"
                })
                .ToListAsync();

            return View("MyReservations", reservations);
        }

        [HttpPost]
        [Authorize(Roles = "User,Dealer,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = GetUserId();

            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
                return NotFound();

            if (User.IsInRole("User") && reservation.UserId != userId)
                return Unauthorized();

            reservation.Status = ReservationStatus.Cancelled;
            await _context.SaveChangesAsync();

            await ActivityLogger.LogAsync(
                _context,
                HttpContext,
                userId,
                "RESERVATION_CANCELLED",
                $"ReservationId={id}"
            );

            return RedirectToAction(User.IsInRole("User") ? "My" : "DealerReservations");
        }

        [Authorize(Roles = "Dealer,Admin")]
        public async Task<IActionResult> DealerReservations()
        {
            var reservations = await _context.Reservations
                .OrderByDescending(r => r.FromDate)
                .Select(r => new DealerReservationViewModel
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    FromDate = r.FromDate,
                    ToDate = r.ToDate,
                    Status = r.Status,
                    CarName = _context.Cars
                        .Where(c => c.Id == r.CarId)
                        .Select(c => c.Brand + " " + c.Model)
                        .FirstOrDefault() ?? "Unknown"
                })
                .ToListAsync();

            return View(reservations);
        }

        [Authorize(Roles = "Dealer,Admin")]
        public async Task<IActionResult> CreateForCustomer()
        {
            ViewBag.Users = await _context.Users.ToListAsync();
            ViewBag.Cars = await _context.Cars.ToListAsync();

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Dealer,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateForCustomer(CreateReservationDto dto)
        {
            var dealerId = GetUserId();

            if (string.IsNullOrEmpty(dto.UserId))
                ModelState.AddModelError("", "User is required");

            var car = await _context.Cars.FindAsync(dto.CarId);

            if (car == null)
                ModelState.AddModelError("", "Car not found");

            if (dto.StartDate >= dto.EndDate)
                ModelState.AddModelError("", "Invalid date range");

            var overlap = await _context.Reservations.AnyAsync(r =>
                r.CarId == dto.CarId &&
                r.Status != ReservationStatus.Cancelled &&
                !(dto.EndDate <= r.FromDate || dto.StartDate >= r.ToDate)
            );

            if (overlap)
                ModelState.AddModelError("", "Car is already reserved");

            if (!ModelState.IsValid)
            {
                ViewBag.Users = await _context.Users.ToListAsync();
                ViewBag.Cars = await _context.Cars.ToListAsync();
                return View(dto);
            }

            await _service.CreateReservationAsync(dto.UserId, dto, dealerId);

            await ActivityLogger.LogAsync(
                _context,
                HttpContext,
                dealerId,
                "RESERVATION_CREATED_BY_DEALER",
                $"UserId={dto.UserId}, CarId={dto.CarId}"
            );

            return RedirectToAction("DealerReservations");
        }

        [HttpPost]
        [Authorize(Roles = "Dealer,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(int id)
        {
            var dealerId = GetUserId();

            await _service.StartReservationAsync(id, dealerId!);

            await ActivityLogger.LogAsync(
                _context,
                HttpContext,
                dealerId,
                "RESERVATION_STARTED",
                $"ReservationId={id}"
            );

            return RedirectToAction("DealerReservations");
        }

        [HttpPost]
        [Authorize(Roles = "Dealer,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var userId = GetUserId();

            await _service.CompleteReservationAsync(id);

            await ActivityLogger.LogAsync(
                _context,
                HttpContext,
                userId,
                "RESERVATION_COMPLETED",
                $"ReservationId={id}"
            );

            return RedirectToAction("DealerReservations");
        }

        [HttpPost]
        [Authorize(Roles = "Dealer,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EndEarly(int id)
        {
            var userId = GetUserId();

            await _service.EndEarlyAsync(id);

            await ActivityLogger.LogAsync(
                _context,
                HttpContext,
                userId,
                "RESERVATION_ENDED_EARLY",
                $"ReservationId={id}"
            );

            return RedirectToAction("DealerReservations");
        }

        public async Task<IActionResult> GetCalendarReservations(Guid carId)
        {
            var userId = GetUserId();

            var reservations = await _context.Reservations
                .Where(r => r.CarId == carId && r.Status != ReservationStatus.Cancelled)
                .Select(r => new
                {
                    title = r.UserId == userId ? "Your reservation" : "Reserved",
                    start = r.FromDate.ToString("yyyy-MM-dd"),
                    end = r.ToDate.AddDays(1).ToString("yyyy-MM-dd"),
                    display = "background",
                    color = r.UserId == userId ? "#1890ff" : "#ff4d4f"
                })
                .ToListAsync();

            return Json(reservations);
        }

        [Authorize(Roles = "Dealer,Admin")]
        public async Task<IActionResult> GetReservations()
        {
            var reservations = await _context.Reservations
                .Where(r => r.Status != ReservationStatus.Cancelled)
                .Select(r => new
                {
                    title = r.CarId.ToString(),
                    start = r.FromDate,
                    end = r.ToDate,
                    color = r.Status == ReservationStatus.Active ? "green" :
                            r.Status == ReservationStatus.Pending ? "orange" :
                            "gray"
                })
                .ToListAsync();

            return Json(reservations);
        }
    }
}