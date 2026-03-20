using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CarRental.Models;
using CarRental.Data;
using CarRental.ViewModels;
using Microsoft.AspNetCore.Http;

namespace CarRental.Services
{
    public class ReservationService : IReservationService
    {
        private readonly ApplicationDbContext _db;
        private readonly CarAvailabilityService _availability;
        private readonly IHttpContextAccessor _http;

        public ReservationService(
            ApplicationDbContext db,
            CarAvailabilityService availability,
            IHttpContextAccessor http)
        {
            _db = db;
            _availability = availability;
            _http = http;
        }

        public async Task CreateReservationAsync(string userId, CreateReservationDto dto, string? dealerId = null)
        {
            if (dto.StartDate >= dto.EndDate)
                throw new InvalidOperationException("Invalid date range");

            var car = await _db.Cars.FindAsync(dto.CarId);
            if (car == null)
                throw new Exception("Car not found");

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var isAvailable = await _availability.IsAvailableAsync(
                    dto.CarId,
                    dto.StartDate,
                    dto.EndDate);

                if (!isAvailable)
                    throw new InvalidOperationException("Car already reserved");

                var days = (dto.EndDate - dto.StartDate).Days;

                var reservation = new Reservation
                {
                    CarId = dto.CarId,

                    // ✅ klient (NAJWAŻNIEJSZE)
                    UserId = userId,

                    // ✅ dealer który tworzy
                    DealerId = dealerId,

                    FromDate = dto.StartDate,
                    ToDate = dto.EndDate,
                    TotalPrice = days * car.PricePerDay,
                    Status = ReservationStatus.Pending
                };

                _db.Reservations.Add(reservation);
                await _db.SaveChangesAsync();

                await ActivityLogger.LogAsync(
                    _db,
                    _http.HttpContext,
                    dealerId ?? userId,
                    "ReservationCreated",
                    $"CarId={dto.CarId}, ForUser={userId}, From={dto.StartDate}, To={dto.EndDate}");

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task StartReservationAsync(int id, string dealerId)
        {
            var r = await _db.Reservations.FindAsync(id);

            if (r == null)
                throw new Exception("Reservation not found");

            if (!string.IsNullOrEmpty(r.DealerId) && r.DealerId != dealerId)
                throw new UnauthorizedAccessException();

            if (r.Status != ReservationStatus.Pending)
                throw new InvalidOperationException("Reservation cannot be started");

            if (DateTime.UtcNow < r.FromDate.AddHours(-1))
                throw new InvalidOperationException("Too early to start reservation");

            r.Status = ReservationStatus.Active;
            r.DealerId = dealerId;

            await _db.SaveChangesAsync();

            await ActivityLogger.LogAsync(_db, _http.HttpContext,
                dealerId, "ReservationStarted", $"ReservationId={id}");
        }

        public async Task CompleteReservationAsync(int id)
        {
            var r = await _db.Reservations.FindAsync(id);

            if (r == null)
                throw new Exception("Reservation not found");

            if (r.Status != ReservationStatus.Active)
                throw new InvalidOperationException("Invalid state");

            r.Status = ReservationStatus.Completed;

            await _db.SaveChangesAsync();

            await ActivityLogger.LogAsync(_db, _http.HttpContext,
                null, "ReservationCompleted", $"ReservationId={id}");
        }

        public async Task EndEarlyAsync(int id)
        {
            var r = await _db.Reservations.FindAsync(id);

            if (r == null)
                throw new Exception("Reservation not found");

            if (r.Status != ReservationStatus.Active)
                throw new InvalidOperationException("Invalid state");

            r.Status = ReservationStatus.Completed;
            r.ToDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            await ActivityLogger.LogAsync(_db, _http.HttpContext,
                null, "ReservationEndedEarly", $"ReservationId={id}");
        }

        public async Task CancelReservationAsync(string userId, int id)
        {
            var r = await _db.Reservations.FindAsync(id);

            if (r == null)
                throw new Exception("Reservation not found");

            if (r.UserId != userId)
                throw new UnauthorizedAccessException();

            if (r.Status == ReservationStatus.Completed)
                throw new InvalidOperationException("Already completed");

            if (r.Status == ReservationStatus.Active)
                throw new InvalidOperationException("Cannot cancel active reservation");

            r.Status = ReservationStatus.Cancelled;

            await _db.SaveChangesAsync();

            await ActivityLogger.LogAsync(_db, _http.HttpContext,
                userId, "ReservationCancelled", $"ReservationId={id}");
        }
    }
}