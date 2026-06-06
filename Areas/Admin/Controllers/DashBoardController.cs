
using Cinema_Project.Models;
using Cinema_Project.Repositories;
using Cinema_Project.Utilities;
using Cinema_Project.ViewModel;
using CinemaProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Cinema_Project.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    [Authorize(Roles = $"{RolesName.ADMIN} ,{RolesName.SUPER_ADMIN}")]
    public class DashBoardController : Controller
    {
        private readonly IRepository<Category> _categoryRepository;
        public readonly IRepository<Booking> _bookingRepository;
        private readonly IRepository<Cinema> _cinemaRepository;
        private readonly IRepository<Movie> _movieRepository;
        private readonly IRepository<Actor> _actorRepository;

        public DashBoardController(
            IRepository<Category> categoryRepository,
            IRepository<Cinema> cinemaRepository,
            IRepository<Movie> movieRepository,
            IRepository<Actor> actorRepository,
            IRepository<Booking> bookingRepository)
        {
            _categoryRepository = categoryRepository;
            _cinemaRepository = cinemaRepository;
            _movieRepository = movieRepository;
            _actorRepository = actorRepository;
            _bookingRepository = bookingRepository;
        }


        public async Task<IActionResult> Index()
            {
                var movies = await _movieRepository.GetAsync(Tracking: false);
                var cinemas = await _cinemaRepository.GetAsync(Tracking: false);
                var actors = await _actorRepository.GetAsync(Tracking: false);
                var categories = await _categoryRepository.GetAsync(Tracking: false);

                var bookings = await _bookingRepository.GetAsync(
                    thenInclude: query => query
                        .Include(e => e.ShowTime)
                            .ThenInclude(st => st.Movie)
                        .Include(e => e.ShowTime)
                            .ThenInclude(st => st.Cinema)
                        .Include(e => e.BookingSeats)
                        .Include(e=>e.PendingSeats),
                        
                    Tracking: false
                );

                var dashboardVM = new DashBoardVM
                {
                    MoviesCount = movies.Count(),
                    CinemasCount = cinemas.Count(),
                    ActorsCount = actors.Count(),
                    CategoriesCount = categories.Count(),

                    TotalBookings = bookings.Count(),
                    PaidBookings = bookings.Count(e => e.PaymentStatus == PaymentStatus.Paid),
                    PendingBookings = bookings.Count(e => e.PaymentStatus == PaymentStatus.Pending),
                    RefundedBookings = bookings.Count(e => e.PaymentStatus == PaymentStatus.Refunded),

                    TotalRevenue = bookings
                        .Where(e => e.PaymentStatus == PaymentStatus.Paid &&
                                    e.BookingStatus == BookingStatus.Confirmed)
                        .Sum(e => e.TotalPrice),

                    LatestBookings = bookings
                        .OrderByDescending(e => e.Id)
                        .Take(6)
                        .ToList()
                };

                return View(dashboardVM);
            }
        }
    }

