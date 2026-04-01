using CarRental.Models;
using CarRental.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarRental.Controllers
{
    [Authorize(Roles = "Dealer,Admin")]
    public class DealerController : Controller
    {
        private readonly ICarService _carService;
        private readonly IReservationService _reservationService;
        private readonly IActivityLogger _logger;

        public DealerController(
            ICarService carService,
            IReservationService reservationService,
            IActivityLogger logger)
        {
            _carService = carService;
            _reservationService = reservationService;
            _logger = logger;
        }

        private string? GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public async Task<IActionResult> Index()
        {
            var cars = await _carService.ListCarsAsync();
            return View(cars);
        }

        public async Task<IActionResult> Dashboard()
        {
            var cars = await _carService.ListCarsAsync();
            return View(cars);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(Guid carId, CarStatus status)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();

            try
            {
                await _carService.UpdateStatusAsync(carId, status);

                await _logger.LogAsync(
                    userId,
                    "CAR_STATUS_UPDATED",
                    $"CarId={carId}, Status={status}"
                );

                TempData["Success"] = "Status updated";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Edit), new { id = carId });
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Car car)
        {
            var userId = GetUserId();

            if (!ModelState.IsValid)
                return View(car);

            await _carService.AddCarAsync(car);

            await _logger.LogAsync(
                userId,
                "CAR_CREATED",
                $"CarId={car.Id}"
            );

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var car = await _carService.GetCarAsync(id);

            if (car == null)
                return NotFound();

            return View(car);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Car updatedCar)
        {
            var userId = GetUserId();

            if (!ModelState.IsValid)
                return View(updatedCar);

            try
            {
                await _carService.UpdateCarAsync(updatedCar);

                await _logger.LogAsync(
                    userId,
                    "CAR_UPDATED",
                    $"CarId={updatedCar.Id}"
                );

                TempData["Success"] = "Car updated";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Edit), new { id = updatedCar.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetUserId();

            try
            {
                await _carService.RemoveCarAsync(id);

                await _logger.LogAsync(
                    userId,
                    "CAR_DELETED",
                    $"CarId={id}"
                );
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DealerReservations()
        {
            var reservations = await _reservationService.GetAllAsync();
            return View(reservations);
        }
    }
}