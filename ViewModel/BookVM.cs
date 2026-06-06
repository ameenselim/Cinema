using Cinema_Project.Models;

namespace Cinema_Project.ViewModel
{
    public class BookVM
    {
        public int ShowTimeId { get; set; }

        public string MovieName { get; set; } = null!;
        public string CinemaName { get; set; } = null!;

        public DateTime StartTime { get; set; }
        public decimal Price { get; set; }

        public List<Seat> Seats { get; set; } = new();
        public List<int> BookedSeatIds { get; set; } = new();
        public List<int> PendingSeatIds { get; set; } = new List<int>();
    }
}
