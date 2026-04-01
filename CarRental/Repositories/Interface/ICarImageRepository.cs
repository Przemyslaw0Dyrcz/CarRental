using CarRental.Models;

namespace CarRental.Repositories.Interface
{
    public interface ICarImageRepository
    {
        Task AddAsync(CarImage image);
    }
}
