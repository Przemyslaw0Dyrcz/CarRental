using System;

namespace CarRental.Models
{
    public enum ReservationStatus
    {
        Pending,
        Active,
        Completed,
        Cancelled,
        NoShow
    }

    public enum PaymentStatus
    {
        Pending,
        Paid,
        Failed
    }

    public class Reservation
    {
        public int Id { get; set; }

        public Guid CarId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? DealerId { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public decimal TotalPrice { get; set; }

        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    }
}