using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarRental.Models;

namespace CarRental.Services
{
    public interface ICarService
    {
        Task<IEnumerable<Car>> ListCarsAsync();
        Task<Car?> GetCarAsync(Guid id);
        Task AddCarAsync(Car car);
        Task UpdateCarAsync(Car car);
        Task RemoveCarAsync(Guid id);
    }
}