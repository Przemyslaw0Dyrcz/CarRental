using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarRental
{
    public class CarSearchQuery
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public Guid? CategoryId { get; set; }
        public decimal? MaxPrice { get; set; }

        public string? Sort { get; set; }
    }
}
