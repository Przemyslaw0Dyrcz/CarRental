using CarRental.Models;

namespace CarRental.ViewModels
{
    public class DealerReservationViewModel
    {
        public int Id { get; set; }

        public string CarName { get; set; } = "";
        public string UserId { get; set; } = "";

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public ReservationStatus Status { get; set; }
    }
}