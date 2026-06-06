namespace Cinema_Project.Models
{
    public class PendingSeat
    {
        public int Id { get; set; }
        public int SeatId { get; set; }
        public Seat Seat { get; set; } = null!;
        public int ShowTimeId { get; set; }
        public ShowTime ShowTime { get; set; } = null!;
        public int BookingId { get; set; }
        public Booking Booking { get; set; } = null!;
        public DateTime ExpireAt { get; set; }
    }
}
