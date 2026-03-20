using System;
using System.Collections.Generic;

namespace CarRental.Models
{
    public enum CarStatus
    {
        Available,
        Reserved,
        Rented,
        Maintenance,
        Disabled
    }

    public class Car
    {
        public Guid Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal PricePerDay { get; set; }

        public double EngineCapacity { get; set; } 
        public int Horsepower { get; set; } 
        public double ZeroToHundred { get; set; } 
        public double FuelConsumption { get; set; } 
        public int Seats { get; set; }
        public int TrunkCapacity { get; set; }

        public CarStatus Status { get; set; } = CarStatus.Available;

        public List<CarImage> Images { get; set; } = new List<CarImage>();

        public string DealerId { get; set; } = string.Empty;
    }
}
