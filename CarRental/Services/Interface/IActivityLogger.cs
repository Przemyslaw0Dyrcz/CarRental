
namespace CarRental.Services.Interface
    {
        public interface IActivityLogger
        {
            Task LogAsync(string? userId, string action, string description);
        }
    }

