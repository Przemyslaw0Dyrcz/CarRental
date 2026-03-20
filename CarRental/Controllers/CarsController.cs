using CarRental.Data;
using CarRental.Models;
using CarRental.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CarRental.Controllers
{
    public class CarsController : Controller
    {
        private readonly ICarService _carService;
        private readonly ApplicationDbContext _context;

        public CarsController(ICarService carService, ApplicationDbContext context)
        {
            _carService = carService;
            _context = context;
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

            if (file == null || file.Length == 0)
                return BadRequest("No file");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(ext))
                return BadRequest("Invalid file type");
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("File too large");

            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(uploads))
                Directory.CreateDirectory(uploads);

            var fileName = Guid.NewGuid() + ext;
            var path = Path.Combine(uploads, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var img = new CarImage
            {
                Id = Guid.NewGuid(),
                CarId = carId,
                Url = "/uploads/" + fileName
            };

            _context.CarImages.Add(img);
            await _context.SaveChangesAsync();

            await ActivityLogger.LogAsync(
                _context,
                HttpContext,
                userId,
                "CAR_IMAGE_UPLOADED",
                $"CarId={carId}, File={fileName}"
            );

            return RedirectToAction("Details", new { id = carId });
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