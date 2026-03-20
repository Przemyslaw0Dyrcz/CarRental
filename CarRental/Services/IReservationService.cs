using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarRental.Models;
using CarRental.ViewModels;

namespace CarRental.Services
{
    public interface IReservationService
    {
        Task CreateReservationAsync(string userId, CreateReservationDto dto, string? dealerId = null);
        Task StartReservationAsync(int id, string dealerId);
        Task CompleteReservationAsync(int id);
        Task EndEarlyAsync(int id);
        Task CancelReservationAsync(string userId, int id);
    }
}
