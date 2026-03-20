using System;

namespace CarRental.ViewModels
{
    public class CreateCarDto
    {
        public Guid CarModelId { get; set; }
        public string VIN { get; set; } = null!;
        public string? LicensePlate { get; set; }
        public string? Location { get; set; }
        public decimal PricePerDay { get; set; }
        public double EngineCapacity { get; set; }
        public int Horsepower { get; set; }
        public double ZeroToHundred { get; set; }
        public double FuelConsumption { get; set; }
        public int Seats { get; set; }
        public int TrunkCapacity { get; set; }

        public string? ImageUrl { get; set; }
    }
}
