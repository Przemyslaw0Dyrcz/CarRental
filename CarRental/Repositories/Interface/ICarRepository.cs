using CarRental.Models;

namespace CarRental.Repositories.Interface
{
    public interface ICarRepository
    {
        Task<List<Car>> GetAllAsync();
        Task<Car?> GetByIdAsync(Guid id);
        Task AddAsync(Car car);
        Task UpdateAsync(Car car);
        Task DeleteAsync(Car car);
        Task SaveChangesAsync();
        Task<bool> HasActiveReservations(Guid carId);
    }
}
