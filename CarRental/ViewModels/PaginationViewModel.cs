
namespace CarRental.ViewModels
{
    public class PaginationViewModel
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public string Action { get; set; }
        public string Search { get; set; }
    }
}
