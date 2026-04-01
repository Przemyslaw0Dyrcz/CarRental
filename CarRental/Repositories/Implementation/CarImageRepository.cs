using CarRental.Data;
using CarRental.Models;
using CarRental.Repositories.Interface;

namespace CarRental.Repositories.Implementation
{
    public class CarImageRepository : ICarImageRepository
    {
        private readonly ApplicationDbContext _context;

        public CarImageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(CarImage image)
        {
            await _context.CarImages.AddAsync(image);
        }
    }
}
