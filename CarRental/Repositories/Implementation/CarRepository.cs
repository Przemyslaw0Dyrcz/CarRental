using CarRental.Data;
using CarRental.Models;
using CarRental.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
namespace CarRental.Repositories.Implementation
{
    public class CarRepository : ICarRepository
    {
        private readonly ApplicationDbContext _context;

        public CarRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Car>> GetAllAsync()
            => await _context.Cars.ToListAsync();

        public async Task<Car?> GetByIdAsync(Guid id)
            => await _context.Cars.FirstOrDefaultAsync(c => c.Id == id);

        public async Task AddAsync(Car car)
            => await _context.Cars.AddAsync(car);

        public Task UpdateAsync(Car car)
        {
            _context.Cars.Update(car);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Car car)
        {
            _context.Cars.Remove(car);
            return Task.CompletedTask;
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasActiveReservations(Guid carId)
            => await _context.Reservations.AnyAsync(r =>
                r.CarId == carId &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Active));
    }
}
