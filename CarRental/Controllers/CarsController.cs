using CarRental.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarRental.Controllers
{
    public class CarsController : Controller
    {
        private readonly ICarService _carService;

        public CarsController(ICarService carService)
        {
            _carService = carService;
        }

        private string? GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        [HttpPost]
        [Authorize(Roles = "Dealer,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImage(Guid carId, IFormFile file)
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized();

            try
            {
                await _carService.UploadImageAsync(carId, file, userId);
                return RedirectToAction("Details", new { id = carId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public async Task<IActionResult> Index(string q, decimal? minPrice, decimal? maxPrice)
        {
            var cars = await _carService.ListCarsAsync();

            var filtered = cars.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
                filtered = filtered.Where(c => c.Brand.Contains(q) || c.Model.Contains(q));

            if (minPrice.HasValue)
                filtered = filtered.Where(c => c.PricePerDay >= minPrice.Value);

            if (maxPrice.HasValue)
                filtered = filtered.Where(c => c.PricePerDay <= maxPrice.Value);

            return View(filtered.ToList());
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var car = await _carService.GetCarAsync(id);

            if (car == null)
                return NotFound();

            return View(car);
        }
    }
}