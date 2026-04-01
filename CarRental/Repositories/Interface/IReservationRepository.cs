using CarRental.Models;
using CarRental.ViewModels;

namespace CarRental.Repositories.Interface
{
    public interface IReservationRepository
    {
        Task<Reservation?> GetByIdAsync(int id);
        Task AddAsync(Reservation reservation);
        Task<bool> ExistsOverlap(Guid carId, DateTime from, DateTime to);
        Task<List<Reservation>> GetAllAsync();
        Task<List<MyReservationViewModel>> GetByUserIdAsync(string userId);
        Task SaveChangesAsync();
    }
}
