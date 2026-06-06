using Cinema_Project.Models;
using Cinema_Project.Repositories;
using Cinema_Project.Utilities;
using Cinema_Project.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
using System.Threading.Tasks;

namespace Cinema_Project.Areas.Customer.Controllers
{
    [Area(SD.CUSTOMER_AREA)]
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IRepository<ShowTime> _showTimeRepository;
        private readonly IRepository<PendingSeat> _pendingSeatRepository;
        private readonly IRepository<Booking> _bookingRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<BookingSeat> _bookingSeatRepository;
        private readonly IRepository<Seat> _seatRepository;
        private readonly IConfiguration _configuration;

        public BookingController(IRepository<ShowTime> showTimeRepository,
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
        public async Task<IActionResult> Book(int showTimeId)
        {
            TempData["info_notification"] = "The Yellow Chairs are temporarily reserved for 15 minutes";
            await RemoveExpiredPendingSeats();

            var showTime = await _showTimeRepository.GetOneAsync(e => e.Id == showTimeId,
                includes: [c => c.Cinema, e => e.Movie], Tracking: false);

            if (showTime == null) return NotFound();

            var bookingDeadLine = showTime.StartTime.AddMinutes(-15);
            if (DateTime.Now >= bookingDeadLine)
            {
                TempData["error_notification"] = "Booking is closed for this show time.";
                return RedirectToAction("Details", "Home", new { id = showTime.MovieId });
            }

            var seats = await _seatRepository.GetAsync(e => e.CinemaId == showTime.CinemaId, Tracking: false);


            var bookedSeats = await _bookingSeatRepository.GetAsync(
                                    e => e.Booking.ShowTimeId == showTimeId
                                    && e.Booking.BookingStatus == BookingStatus.Confirmed
                                    && e.Booking.PaymentStatus == PaymentStatus.Paid,
                                    includes: [b => b.Booking],
                                    Tracking: false);
                                    

            var pendingSeat = await _pendingSeatRepository.GetAsync(e => e.Booking.ShowTimeId == showTime.Id && e.ExpireAt > DateTime.Now);
            var bookVM = new BookVM()
            {
                ShowTimeId = showTime.Id,
                StartTime = showTime.StartTime,
                CinemaName = showTime.Cinema.Name,
                MovieName = showTime.Movie.Name,
                Price = showTime.Price,
                Seats = seats.ToList(),
                BookedSeatIds = bookedSeats.Select(e => e.SeatId).ToList(),
                PendingSeatIds = pendingSeat.Select(e => e.SeatId).ToList()
            };

            return View(bookVM);
        }
        [HttpPost]
        public async Task<IActionResult> ConfirmBooking(int showTimeId, List<int> selectedSeatIds)
        {
            if (selectedSeatIds is null || !selectedSeatIds.Any())
            {
                TempData["error_notification"] = "Please select at least one seat.";
                return RedirectToAction(nameof(Book), new { showTimeId });
            }
            selectedSeatIds = selectedSeatIds.Distinct().ToList();

            var showTime = await _showTimeRepository.GetOneAsync(e => e.Id == showTimeId);
            if (showTime == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var bookingDeadLine = showTime.StartTime.AddMinutes(-15);
            if (DateTime.Now > bookingDeadLine)
            {
                TempData["error_notification"] = "Booking is closed for this show time.";
                return RedirectToAction("Details", "Home", new { id = showTime.MovieId });
            }

            var bookedSeats = await _bookingSeatRepository.GetAsync(e => e.Booking.ShowTimeId == showTime.Id
                            && selectedSeatIds.Contains(e.SeatId)
                            && e.Booking.BookingStatus == BookingStatus.Confirmed
                            && e.Booking.PaymentStatus == PaymentStatus.Paid
                            , includes: [e => e.Booking]);

            if (bookedSeats.Any())
            {
                TempData["error_notification"] = "One or more selected seats are already booked.";
                return RedirectToAction(nameof(Book), new { showTimeId });
            }

            var pendingSeatActive = await _pendingSeatRepository.GetAsync(e => e.ShowTimeId == showTime.Id && selectedSeatIds.Contains(e.SeatId) && e.ExpireAt > DateTime.Now);

            if (pendingSeatActive.Any())
            {
                TempData["error_notification"] = "One or more selected seats are currently being booked by another user. Please try again.";
                return RedirectToAction(nameof(Book), new { showTimeId });
            }
            var expireAt = DateTime.Now.AddMinutes(15);

            if (expireAt > showTime.StartTime)
            {
                expireAt = showTime.StartTime;
            }

            Booking booking = new Booking()
            {
                ApplicationUserId = user.Id,
                ShowTimeId = showTime.Id,
                TotalPrice = showTime.Price * selectedSeatIds.Count(),
                BookingStatus = BookingStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
            };

            await _bookingRepository.CreateAsync(booking);
            await _bookingRepository.Commit();

            foreach (var seatId in selectedSeatIds)
            {
                var pendingSeat = new PendingSeat()
                {
                    SeatId = seatId,
                    ShowTimeId = showTime.Id,
                    BookingId = booking.Id,
                    ExpireAt = DateTime.Now.AddMinutes(15)
                };
                await _pendingSeatRepository.CreateAsync(pendingSeat);
            }
            await _pendingSeatRepository.Commit();

            TempData["success_notification"] = "Booking completed successfully.";

            return RedirectToAction(nameof(Pay), new
            {
                bookingId = booking.Id,
            });
        }
        public async Task<IActionResult> Pay(int bookingId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var booking = await _bookingRepository.GetOneAsync(e => e.Id == bookingId && e.ApplicationUserId == user.Id,
                            includes: [e => e.ShowTime, e => e.ShowTime.Movie]);
            if (booking == null) return NotFound();

            if (booking.PaymentStatus == PaymentStatus.Paid)
            {
                TempData["error_notification"] = "Booking has already been paid.";
                return RedirectToAction(nameof(MyBookings));
            }
            if (booking.BookingStatus != BookingStatus.Pending)
            {
                TempData["error_notification"] = "Booking is not in a valid state for payment.";
                return RedirectToAction(nameof(MyBookings));
            }
            var pendingSeats = await _pendingSeatRepository.GetAsync(e => e.BookingId == booking.Id, includes: [e => e.Seat], Tracking: false);

            if (!pendingSeats.Any())
            {
                TempData["error_notification"] = "No pending seats found.";
                return RedirectToAction(nameof(Book), new { showTimeId = booking.ShowTimeId });
            }

            bool expiredSeats = pendingSeats.Any(e => e.ExpireAt < DateTime.Now);
            if (expiredSeats)
            {

                booking.BookingStatus = BookingStatus.Cancelled;
                booking.PaymentStatus = PaymentStatus.Failed;

                foreach (var pendingSeat in pendingSeats)
                {
                    _pendingSeatRepository.Delete(pendingSeat);
                }

                await _pendingSeatRepository.Commit();
                await _bookingRepository.Commit();

                TempData["error_notification"] = "Booking time expired. Please select seats again.";
                return RedirectToAction(nameof(Book), new { showTimeId = booking.ShowTimeId });
            }

            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "egp",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = booking.ShowTime.Movie.Name,
                            Description = $"Seats count: {pendingSeats.Count()}"
                        },
                        UnitAmount = (long)booking.ShowTime.Price *100
                    },
                    Quantity =pendingSeats.Count()
                    },
                },
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/customer/checkout/success?bookingid={booking.Id}",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/customer/checkout/cancel?bookingid={booking.Id}"
            };            
            var service = new SessionService();
            var session = service.Create(options);
            booking.SessionId = session.Id;

            await _bookingRepository.Commit();
            return Redirect(session.Url);
        }





        public async Task<IActionResult> MyBookings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            //var bookings = await _bookingRepository.GetAsync(e => e.ApplicationUserId == user.Id,
            //    includes: [e => e.ShowTime,
            //                    e => e.ShowTime.Movie,
            //                    e => e.ShowTime.Cinema,
            //                    e => e.BookingSeats,
            //                    e => e.BookingSeats.Select(bs => bs.Seat),
            //                    e => e.PendingSeats,
            //                    e => e.PendingSeats.Select(ps => ps.Seat)]);

            var bookings = await _bookingRepository.GetAsync(
                    e => e.ApplicationUserId == user.Id,
                    thenInclude: query => query
                        .Include(e => e.ShowTime)
                    .ThenInclude(st => st.Movie)
                        .Include(e => e.ShowTime)
                    .ThenInclude(st => st.Cinema)
                        .Include(e => e.BookingSeats)
                    .ThenInclude(bs => bs.Seat)
                        .Include(e => e.PendingSeats)
                    .ThenInclude(ps => ps.Seat),
                    Tracking: false);

            return View(bookings);
        }

        private async Task RemoveExpiredPendingSeats()
        {
            var pendingSeatExpire = await _pendingSeatRepository.GetAsync(e => e.ExpireAt <= DateTime.Now);
            if (!pendingSeatExpire.Any())
                return;

            foreach (var pendingSeat in pendingSeatExpire)
            {
                if (pendingSeat.Booking is not null &&
                    pendingSeat.Booking.BookingStatus == BookingStatus.Pending &&
                    pendingSeat.Booking.PaymentStatus == PaymentStatus.Pending)
                {
                    pendingSeat.Booking.BookingStatus = BookingStatus.Cancelled;
                    pendingSeat.Booking.PaymentStatus = PaymentStatus.Failed;
                }

                _pendingSeatRepository.Delete(pendingSeat);
            }

            await _pendingSeatRepository.Commit();
            await _bookingRepository.Commit();
        }
    }
}
