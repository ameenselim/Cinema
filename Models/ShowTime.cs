using CinemaProject.Models;

namespace Cinema_Project.Models
{
    public class ShowTime
    {
        public int Id { get; set; }

        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;

        public int CinemaId { get; set; }
        public Cinema Cinema { get; set; } = null!;

        public DateTime StartTime { get; set; }

        public decimal Price { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<PendingSeat> PendingSeats { get; set; } = new List<PendingSeat>();
    }
}
