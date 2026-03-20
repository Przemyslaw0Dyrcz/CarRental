using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarRental
{
    public class ReservationDto
    {
        public Guid Id { get; set; }
        public Guid CarId { get; set; }
        public string CarBrand { get; set; } = null!;
        public string CarModel { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = null!;
        public decimal TotalPrice { get; set; }
    }
}
