using Cinema_Project.Models;
using Cinema_Project.Repositories;
using Cinema_Project.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using System.Threading.Tasks;

namespace Cinema_Project.Areas.Customer.Controllers
{
    [Area(SD.CUSTOMER_AREA)]
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly IRepository<ShowTime> _showTimeRepository;
        private readonly IRepository<PendingSeat> _pendingSeatRepository;
        private readonly IRepository<Booking> _bookingRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<BookingSeat> _bookingSeatRepository;
        private readonly IRepository<Seat> _seatRepository;
        private readonly IConfiguration _configuration;

        public CheckoutController(IRepository<ShowTime> showTimeRepository,
            IRepository<PendingSeat> pendingSeatRepository, IRepository<Booking> bookingRepository,
            UserManager<ApplicationUser> userManager, IRepository<BookingSeat> bookingSeatRepository,
            IRepository<Seat> seatRepository, IConfiguration configuration)
        {
            _showTimeRepository = showTimeRepository;
            _pendingSeatRepository = pendingSeatRepository;
            _bookingRepository = bookingRepository;
            _userManager = userManager;
            _bookingSeatRepository = bookingSeatRepository;
            _seatRepository = seatRepository;
            _configuration = configuration;
        }
        public async Task<IActionResult> Success(int bookingId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound();

            var booking = await _bookingRepository.GetOneAsync(e => e.Id == bookingId && e.ApplicationUserId == user.Id,
                                includes: [e => e.ShowTime, e => e.ShowTime.Movie, e => e.ShowTime.Cinema]);
            if (booking is null) return NotFound();

            if (booking.PaymentStatus == PaymentStatus.Paid)
            {
                TempData["success_notification"] = "Booking already paid.";
                return RedirectToAction(nameof(BookingController.MyBookings), SD.BOOKING_CONTROLLER);
            }

            if (string.IsNullOrEmpty(booking.SessionId))
            {
                TempData["error_notification"] = "Payment session not found.";
                return RedirectToAction(nameof(BookingController.MyBookings), SD.BOOKING_CONTROLLER);
            }

            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];

            var services = new SessionService();
            var session = services.Get(booking.SessionId);

            if (session is null) return NotFound();

            if (session.PaymentStatus != "paid")
            {
                TempData["error_notification"] = "Payment not completed.";
                return RedirectToAction(nameof(BookingController.Pay), SD.BOOKING_CONTROLLER, new { bookingId = booking.Id });
            }
            var pendingSeats = await _pendingSeatRepository.GetAsync(e => e.BookingId == booking.Id);

            if (!pendingSeats.Any())
            {
                TempData["error_notification"] = "No pending seats found.";
                return RedirectToAction(nameof(BookingController.Book), SD.BOOKING_CONTROLLER, new { showTimeId = booking.ShowTimeId });
            }
            if (pendingSeats.Any(e => e.ExpireAt <= DateTime.Now))
            {
                booking.PaymentStatus = PaymentStatus.Failed;
                booking.BookingStatus = BookingStatus.Cancelled;

                foreach (var item in pendingSeats)
                {
                    _pendingSeatRepository.Delete(item);
                }
                await _pendingSeatRepository.Commit();
                await _bookingRepository.Commit();

                TempData["error_notification"] = "Booking time expired. Please select seats again.";
                return RedirectToAction(nameof(BookingController.Book), SD.BOOKING_CONTROLLER, new { showTimeId = booking.ShowTimeId });
            }
            var seatIds = pendingSeats.Select(e => e.SeatId);
            var bookedSeats = await _bookingSeatRepository.GetAsync(e => e.Booking.ShowTimeId == booking.ShowTimeId && seatIds.Contains(e.SeatId)
                                                    && e.Booking.BookingStatus == BookingStatus.Confirmed && e.Booking.PaymentStatus == PaymentStatus.Paid);

            if (bookedSeats.Any())
            {
                booking.PaymentStatus = PaymentStatus.Failed;
                booking.BookingStatus = BookingStatus.Cancelled;

                foreach (var item in pendingSeats)
                {
                    _pendingSeatRepository.Delete(item);
                }
                await _pendingSeatRepository.Commit();
                await _bookingRepository.Commit();
                TempData["error_notification"] = "Sorry, one or more seats have already been booked.";
                return RedirectToAction(nameof(BookingController.Book), SD.BOOKING_CONTROLLER, new { showTimeId = booking.ShowTimeId });
            }

            foreach (var item in pendingSeats)
            {
                var bookingSeat = new BookingSeat()
                {
                    BookingId = booking.Id,
                    SeatId = item.SeatId
                };
                await _bookingSeatRepository.CreateAsync(bookingSeat);
            }
            await _bookingSeatRepository.Commit();

            booking.PaymentStatus = PaymentStatus.Paid;
            booking.BookingStatus = BookingStatus.Confirmed;
            booking.TransactionId = session.PaymentIntentId;

            foreach (var item in pendingSeats)
            {
                _pendingSeatRepository.Delete(item);
            }
            await _pendingSeatRepository.Commit();
            await _bookingRepository.Commit();

            TempData["success_notification"] = "Payment completed and seats booked successfully.";
            return View(booking);
        }


        public async Task<IActionResult> Cancel(int bookingId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var booking = await _bookingRepository.GetOneAsync(e => e.Id == bookingId && e.ApplicationUserId == user.Id,
                            includes: [e => e.ShowTime, e => e.ShowTime.Movie, e => e.ShowTime.Cinema]);
            if (booking is null) return NotFound();

            if (booking.PaymentStatus == PaymentStatus.Paid)
            {
                TempData["error_notification"] = "Paid booking cannot be cancelled from here.";
                return RedirectToAction(nameof(BookingController.MyBookings), SD.BOOKING_CONTROLLER);

            }
            var pendingSeat = await _pendingSeatRepository.GetAsync(e => e.BookingId == booking.Id);
            foreach (var item in pendingSeat)
            {
                _pendingSeatRepository.Delete(item);
            }
            await _pendingSeatRepository.Commit();
            booking.BookingStatus = BookingStatus.Cancelled;
            booking.PaymentStatus = PaymentStatus.Failed;

            await _bookingRepository.Commit();

            TempData["success_notification"] = "Booking cancelled successfully.";

            return View(booking);
        }


        public async Task<IActionResult> Refund(int bookingId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound();

            var booking = await _bookingRepository.GetOneAsync(
                                e => e.Id == bookingId && e.ApplicationUserId == user.Id,
                                thenInclude: query => query
                                .Include(e => e.ShowTime)
                                    .ThenInclude(st => st.Movie)
                                .Include(e => e.ShowTime)
                                    .ThenInclude(st => st.Cinema)
                                .Include(e => e.BookingSeats)
                                    .ThenInclude(bs => bs.Seat));
                                                                            
            if (booking is null) return NotFound();

            if (booking.PaymentStatus != PaymentStatus.Paid || booking.BookingStatus != BookingStatus.Confirmed)
            {
                TempData["error_notification"] = "Only paid and confirmed bookings can be refunded.";
                return RedirectToAction(nameof(BookingController.MyBookings), SD.BOOKING_CONTROLLER);
            }

            var refundDeadLine = DateTime.Now.AddHours(24);
            if (booking.ShowTime.StartTime <= refundDeadLine)
            {
                TempData["error_notification"] = "Refund is only allowed more than 24 hours before the show time.";
                return RedirectToAction(nameof(BookingController.MyBookings), SD.BOOKING_CONTROLLER);
            }
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
            string? PaymentIntentId = booking.TransactionId;

            if (string.IsNullOrEmpty(PaymentIntentId))
            {
                if (string.IsNullOrEmpty(booking.SessionId))
                {
                    TempData["error_notification"] = "Payment session not found.";
                    return RedirectToAction(nameof(BookingController.MyBookings), SD.BOOKING_CONTROLLER);
                }
                var services = new SessionService();
                var session = services.Get(booking.SessionId);

                if (session is null || string.IsNullOrEmpty(session.PaymentIntentId))
                {
                    TempData["error_notification"] = "Payment transaction not found.";
                    return RedirectToAction(nameof(BookingController.MyBookings), SD.BOOKING_CONTROLLER);
                }
                PaymentIntentId = session.PaymentIntentId;
                booking.TransactionId = session.PaymentIntentId;
            }

            //var refundService = new RefundService();

            var options = new RefundCreateOptions
            {
                PaymentIntent = PaymentIntentId,
                Amount = (long)((booking.TotalPrice - 10) * 100),
                Reason = "requested_by_customer"
            };
            var service = new RefundService();
            var refund = service.Create(options);

            if (refund.Status != "succeeded")
            {
                TempData["error_notification"] = "Refund request was not completed.";
                return RedirectToAction(nameof(BookingController.MyBookings), SD.BOOKING_CONTROLLER);
            }
            booking.PaymentStatus = PaymentStatus.Refunded;
            booking.BookingStatus = BookingStatus.Refunded;

            if (booking.BookingSeats != null && booking.BookingSeats.Any())
            {
                foreach (var bookingSeat in booking.BookingSeats.ToList())
                {
                    _bookingSeatRepository.Delete(bookingSeat);
                }
            }
            await _bookingSeatRepository.Commit();
            await _bookingRepository.Commit();

            TempData["success_notification"] = "Refund completed successfully.";

            return View(booking);
        }
    }
}
