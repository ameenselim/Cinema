using Cinema_Project.Models;
using Cinema_Project.Repositories;
using Cinema_Project.Utilities;
using Cinema_Project.ViewModel;
using CinemaProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Cinema_Project.Areas.Customer.Controllers
{
    [Area(SD.CUSTOMER_AREA)]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IRepository<Movie> _MovieRepository;
        private readonly IRepository<ShowTime> _showTimeRepository;

        public HomeController(IRepository<Movie> movieRepository ,IRepository<ShowTime> showTimeRepository)
        {
            _MovieRepository = movieRepository;
            _showTimeRepository = showTimeRepository;
        }
        public async Task<IActionResult> Index(string? query)
        {
            var movies = await _MovieRepository.GetAsync(includes: [ e=>e.Category]);

            if (!string.IsNullOrEmpty(query))
            {
                movies = movies.Where(m => m.Name.ToLower().Contains(query.ToLower())).ToList();
            }

            ViewBag.Query = query;

            return View(movies);
        }

        public async Task<IActionResult> Details(int id)
        {
            var movie = await _MovieRepository.GetOneAsync(e => e.Id == id ,includes: [e=>e.Cinema ,e=>e.Category]);
            if (movie == null) return NotFound();

            var showTimes =  await _showTimeRepository.GetAsync(e => e.MovieId == movie.Id && e.StartTime >= DateTime.Now
                                              , includes: [e=>e.Cinema]);

            return View(new MovieDetailsVM()
            {
                Movie = movie,
                ShowTimes = showTimes.ToList(),
            });

        }
    }
}
