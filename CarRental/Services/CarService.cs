using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CarRental.Data;
using CarRental.Models;

namespace CarRental.Services
{
    public class CarService : ICarService
    {
        private readonly ApplicationDbContext _db;

        public CarService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Car>> ListCarsAsync()
        {
            return await _db.Cars.ToListAsync();
        }

        public async Task<Car?> GetCarAsync(Guid id)
        {
            return await _db.Cars.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddCarAsync(Car car)
        {
            car.Id = Guid.NewGuid();
            _db.Cars.Add(car);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateCarAsync(Car car)
        {
            var existing = await _db.Cars.FirstOrDefaultAsync(c => c.Id == car.Id);

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

            await _db.SaveChangesAsync();
        }

        public async Task RemoveCarAsync(Guid id)
        {
            var car = await _db.Cars.FirstOrDefaultAsync(x => x.Id == id);

            if (car == null)
                return;

            var hasActiveReservations = await _db.Reservations.AnyAsync(r =>
                r.CarId == id &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Active));

            if (hasActiveReservations)
                throw new Exception("Cannot delete car with active reservations");

            _db.Cars.Remove(car);
            await _db.SaveChangesAsync();
        }
    }
}