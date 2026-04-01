using CarRental.Data;
using CarRental.Models;
using CarRental.Repositories.Interface;
using CarRental.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Repositories.Implementation
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly ApplicationDbContext _context;

        public ReservationRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<Reservation?> GetByIdAsync(int id)
            => await _context.Reservations.FindAsync(id);

        public async Task AddAsync(Reservation reservation)
            => await _context.Reservations.AddAsync(reservation);

        public async Task<bool> ExistsOverlap(Guid carId, DateTime from, DateTime to)
            => await _context.Reservations.AnyAsync(r =>
                r.CarId == carId &&
                r.Status != ReservationStatus.Cancelled &&
                !(to <= r.FromDate || from >= r.ToDate));
        public async Task<List<MyReservationViewModel>> GetByUserIdAsync(string userId)
        {
            return await _context.Reservations
                .Where(r => r.UserId == userId)
                .Join(
                    _context.Cars,
                    r => r.CarId,
                    c => c.Id,
                    (r, c) => new MyReservationViewModel
                    {
                        Id = r.Id,
                        CarName = c.Brand + " " + c.Model,
                        FromDate = r.FromDate,
                        ToDate = r.ToDate,
                        TotalPrice = r.TotalPrice,
                        Status = r.Status,
                        PaymentStatus = r.PaymentStatus
                    }
                )
                .ToListAsync();
        }
        public async Task<List<Reservation>> GetAllAsync()
        {
            return await _context.Reservations.ToListAsync();
        }
    }

}