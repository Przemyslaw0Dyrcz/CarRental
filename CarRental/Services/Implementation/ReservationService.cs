using CarRental.Data;
using CarRental.Models;
using CarRental.Repositories.Interface;
using CarRental.Services.Interface;
using CarRental.ViewModels;
using Microsoft.AspNetCore.Http;

namespace CarRental.Services.Implementation
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _repo;
        private readonly IActivityLogger _logger;

        public ReservationService(
            IReservationRepository repo,
            IActivityLogger logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task CreateReservationAsync(string userId, CreateReservationDto dto, string? dealerId = null)
        {
            if (dto.StartDate >= dto.EndDate)
                throw new InvalidOperationException("Invalid date range");

            if (await _repo.ExistsOverlap(dto.CarId, dto.StartDate, dto.EndDate))
                throw new InvalidOperationException("Car already reserved");

            var days = (dto.EndDate - dto.StartDate).Days;

            var reservation = new Reservation
            {
                CarId = dto.CarId,
                UserId = userId,
                DealerId = dealerId,
                FromDate = dto.StartDate,
                ToDate = dto.EndDate,
                TotalPrice = days * dto.PricePerDay,
                Status = ReservationStatus.Pending
            };

            await _repo.AddAsync(reservation);
            await _repo.SaveChangesAsync();
            await _logger.LogAsync(
                dealerId ?? userId,
                "RESERVATION_CREATED",
                $"CarId={dto.CarId}, From={dto.StartDate}, To={dto.EndDate}"
            );
        }

        public async Task StartReservationAsync(int id, string dealerId)
        {
            var r = await _repo.GetByIdAsync(id);

            if (r == null)
                throw new Exception("Reservation not found");

            if (!string.IsNullOrEmpty(r.DealerId) && r.DealerId != dealerId)
                throw new UnauthorizedAccessException();

            if (r.Status != ReservationStatus.Pending)
                throw new InvalidOperationException("Invalid state");

            r.Status = ReservationStatus.Active;
            r.DealerId = dealerId;
            await _repo.SaveChangesAsync();
            await _logger.LogAsync(
                dealerId,
                "RESERVATION_STARTED",
                $"ReservationId={id}"
            );
        }
        public async Task<IEnumerable<MyReservationViewModel>> GetUserReservationsAsync(string userId)
        {
            return await _repo.GetByUserIdAsync(userId);
        }
        public async Task CompleteReservationAsync(int id)
        {
            var r = await _repo.GetByIdAsync(id);

            if (r == null)
                throw new Exception("Reservation not found");

            if (r.Status != ReservationStatus.Active)
                throw new InvalidOperationException("Invalid state");

            r.Status = ReservationStatus.Completed;
            await _repo.SaveChangesAsync();
            await _logger.LogAsync(
                r.UserId ?? "unknown",
                "RESERVATION_COMPLETED",
                $"ReservationId={id}"
            );
        }

        public async Task EndEarlyAsync(int id)
        {
            var r = await _repo.GetByIdAsync(id);

            if (r == null)
                throw new Exception("Reservation not found");

            if (r.Status != ReservationStatus.Active)
                throw new InvalidOperationException("Invalid state");

            r.Status = ReservationStatus.Completed;
            r.ToDate = DateTime.UtcNow;
            await _repo.SaveChangesAsync();
            await _logger.LogAsync(
                r.UserId ?? "unknown",
                "RESERVATION_ENDED_EARLY",
                $"ReservationId={id}"
            );
        }

        public async Task CancelReservationAsync(string userId, int id)
        {
            var r = await _repo.GetByIdAsync(id);

            if (r == null)
                throw new Exception("Reservation not found");

            if (r.UserId != userId)
                throw new UnauthorizedAccessException();

            r.Status = ReservationStatus.Cancelled;
            await _repo.SaveChangesAsync();

            await _logger.LogAsync(
                userId,
                "RESERVATION_CANCELLED",
                $"ReservationId={id}"
            );
        }
        
        public async Task<IEnumerable<Reservation>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }
    }
}