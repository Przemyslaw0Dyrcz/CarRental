using CarRental.Services.Interface;
using CarRental.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarRental.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly IReservationService _service;

        public ReservationsController(IReservationService service)
        {
            _service = service;
        }

        private string? GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        [HttpGet]
        [Authorize(Roles = "User")]
        public IActionResult Create(Guid carId)
        {
            var vm = new CreateReservationViewModel
            {
                CarId = carId
            };

            return View(vm);
        }

        [HttpGet]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> My()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            var reservations = await _service.GetUserReservationsAsync(userId);

            return View(reservations);
        }


        [HttpPost]
        [Authorize(Roles = "User")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReservationViewModel vm)
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized();

            if (!ModelState.IsValid)
                return View(vm);

            var dto = new CreateReservationDto
            {
                CarId = vm.CarId,
                StartDate = vm.StartDate!.Value,
                EndDate = vm.EndDate!.Value,
                PricePerDay = (int)vm.PricePerDay
            };

            await _service.CreateReservationAsync(userId, dto);

            return RedirectToAction("My");
        }

        [HttpPost]
        [Authorize(Roles = "User,Dealer,Admin")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized();

            await _service.CancelReservationAsync(userId, id);

            return RedirectToAction("My");
        }

        [HttpPost]
        [Authorize(Roles = "Dealer,Admin")]
        public async Task<IActionResult> Start(int id)
        {
            var dealerId = GetUserId();

            if (dealerId == null)
                return Unauthorized();

            await _service.StartReservationAsync(id, dealerId);

            return RedirectToAction("DealerReservations");
        }

        [HttpPost]
        [Authorize(Roles = "Dealer,Admin")]
        public async Task<IActionResult> Complete(int id)
        {
            await _service.CompleteReservationAsync(id);

            return RedirectToAction("DealerReservations");
        }

        [HttpPost]
        [Authorize(Roles = "Dealer,Admin")]
        public async Task<IActionResult> EndEarly(int id)
        {
            await _service.EndEarlyAsync(id);

            return RedirectToAction("DealerReservations");
        }
    }
}