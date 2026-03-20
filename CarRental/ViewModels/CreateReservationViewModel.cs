using System;
using System.ComponentModel.DataAnnotations;

namespace CarRental.ViewModels
{
    public class CreateReservationViewModel
    {
        public Guid CarId { get; set; }
        public string Brand { get; set; } = "";
        public string Model { get; set; } = "";
        public int Year { get; set; }
        public decimal PricePerDay { get; set; }
        public int Seats { get; set; }
        public double EngineCapacity { get; set; }

        [Required(ErrorMessage = "Select start date")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "Select end date")]
        public DateTime? EndDate { get; set; }
    }
}