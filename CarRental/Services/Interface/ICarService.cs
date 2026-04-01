using CarRental.Models;
using Microsoft.AspNetCore.Http;

namespace CarRental.Services.Interface;

public interface ICarService
{
    Task<IEnumerable<Car>> ListCarsAsync();
    Task<Car?> GetCarAsync(Guid id);
    Task AddCarAsync(Car car);
    Task UpdateCarAsync(Car car);
    Task RemoveCarAsync(Guid id);
    Task UploadImageAsync(Guid carId, IFormFile file, string? userId);
    Task UpdateStatusAsync(Guid carId, CarStatus status);
}