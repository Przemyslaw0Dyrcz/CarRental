using CarRental.Data;
using CarRental.Models;
using CarRental.Repositories.Interface;
using CarRental.Services.Interface;
using Microsoft.AspNetCore.Http;

namespace CarRental.Services.Implementation
{
    public class CarService : ICarService
    {
        private readonly ICarRepository _repo;
        private readonly ICarImageRepository _imageRepo;


        public CarService(
            ICarRepository repo,
            ICarImageRepository imageRepo)
        {
            _repo = repo;
            _imageRepo = imageRepo;
        }

        public async Task<IEnumerable<Car>> ListCarsAsync()
            => await _repo.GetAllAsync();

        public async Task<Car?> GetCarAsync(Guid id)
            => await _repo.GetByIdAsync(id);

        public async Task AddCarAsync(Car car)
        {
            car.Id = Guid.NewGuid();
            await _repo.AddAsync(car);
            await _repo.SaveChangesAsync();
        }

        public async Task UpdateCarAsync(Car car)
        {
            var existing = await _repo.GetByIdAsync(car.Id);

            if (existing == null)
                throw new Exception("Car not found");

            existing.Brand = car.Brand;
            existing.Model = car.Model;
            existing.Year = car.Year;
            existing.PricePerDay = car.PricePerDay;
            existing.EngineCapacity = car.EngineCapacity;
            existing.Horsepower = car.Horsepower;
            existing.ZeroToHundred = car.ZeroToHundred;
            existing.FuelConsumption = car.FuelConsumption;
            existing.Seats = car.Seats;
            existing.TrunkCapacity = car.TrunkCapacity;
            existing.Status = car.Status;

            await _repo.UpdateAsync(existing);
            await _repo.SaveChangesAsync();
        }

        public async Task RemoveCarAsync(Guid id)
        {
            var car = await _repo.GetByIdAsync(id);

            if (car == null)
                return;

            if (await _repo.HasActiveReservations(id))
                throw new Exception("Cannot delete car with active reservations");

            await _repo.DeleteAsync(car);
            await _repo.SaveChangesAsync();
        }

        public async Task UploadImageAsync(Guid carId, IFormFile file, string? userId)
        {
            if (file == null || file.Length == 0)
                throw new Exception("No file");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(ext))
                throw new Exception("Invalid file type");

            if (file.Length > 5 * 1024 * 1024)
                throw new Exception("File too large");

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

            await _imageRepo.AddAsync(img);
            await _repo.SaveChangesAsync();
        }
        public async Task UpdateStatusAsync(Guid carId, CarStatus status)
        {
            var car = await _repo.GetByIdAsync(carId);

            if (car == null)
                throw new Exception("Car not found");

            if (status == CarStatus.Available &&
                await _repo.HasActiveReservations(carId))
                throw new Exception("Car is currently rented");

            if (status == CarStatus.Maintenance &&
                await _repo.HasActiveReservations(carId))
                throw new Exception("Cannot send to maintenance during active rental");

            car.Status = status;

            await _repo.UpdateAsync(car);
            await _repo.SaveChangesAsync();
        }
    }
}