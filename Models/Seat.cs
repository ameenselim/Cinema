using CinemaProject.Models;

namespace Cinema_Project.Models
{
    public class Seat
    {
        public int Id { get; set; }

        public string SeatNumber { get; set; } = null!; // A1, A2

        public int CinemaId { get; set; }
        public Cinema Cinema { get; set; } = null!;

        public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
        public ICollection<PendingSeat> PendingSeats { get; set; } = new List<PendingSeat>();
    }
}

