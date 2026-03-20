using System;

namespace CarRental.Models
{
    public class CarImage
    {
        public Guid Id { get; set; }
        public Guid CarId { get; set; }
        public string Url { get; set; } = string.Empty;
    }
}
