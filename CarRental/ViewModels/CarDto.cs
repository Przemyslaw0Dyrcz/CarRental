using System;
using System.Collections.Generic;

namespace CarRental.ViewModels
{
    public class CarDto
    {
        public Guid Id { get; set; }
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public int Year { get; set; }
        public decimal PricePerDay { get; set; }
        public string? Location { get; set; }

        // new fields mirrored from model
        public double EngineCapacity { get; set; }
        public int Horsepower { get; set; }
        public double ZeroToHundred { get; set; }
        public double FuelConsumption { get; set; }
        public int Seats { get; set; }
        public int TrunkCapacity { get; set; }

        public string Status { get; set; } = "Available";

        public List<string>? ImageUrls { get; set; }
    }
}