using CinemaProject.Models;

namespace Cinema_Project.Models
{
    public enum BookingStatus
    {
        Pending =1,
        Confirmed =2,
        Cancelled = 3,
        Refunded = 4
    }
    public enum PaymentStatus
    {
        Pending = 1,
        Paid = 2,
        Failed = 3,
        Refunded = 4
    }
    public enum PaymentMethod
    {
        Visa = 1,
        COD,
    }

    public class Booking
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; } = null!;
        public ApplicationUser ApplicationUser { get; set; } = null!;

        public int ShowTimeId { get; set; }
        public ShowTime ShowTime { get; set; } = null!;

        public DateTime BookingDate { get; set; } = DateTime.Now;

        public decimal TotalPrice { get; set; }

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public BookingStatus BookingStatus { get; set; } = BookingStatus.Pending;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Visa;

        public string SessionId { get; set; } = string.Empty;
        public string? TransactionId { get; set; }

        public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
        public ICollection<PendingSeat> PendingSeats { get; set; } = new List<PendingSeat>();
    }
}
