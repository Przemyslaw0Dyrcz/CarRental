using CarRental.Models;

namespace CarRental.ViewModels
{
    public class MyReservationViewModel
    {
        public int Id { get; set; }

        public string CarName { get; set; } = "";

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public decimal TotalPrice { get; set; }

        public ReservationStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
    }
}