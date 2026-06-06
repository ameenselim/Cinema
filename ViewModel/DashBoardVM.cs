
using Cinema_Project.Models;

namespace Cinema_Project.ViewModel
{
    public class DashBoardVM
    {
        public int MoviesCount { get; set; }
        public int CinemasCount { get; set; }
        public int ActorsCount { get; set; }
        public int CategoriesCount { get; set; }

        public int TotalBookings { get; set; }
        public int PaidBookings { get; set; }
        public int PendingBookings { get; set; }
        public int RefundedBookings { get; set; }

        public decimal TotalRevenue { get; set; }

        public IEnumerable<Booking> LatestBookings { get; set; } = new List<Booking>();

    }
}
