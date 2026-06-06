using Cinema_Project.Models;
using Cinema_Project.Repositories;
using Cinema_Project.Utilities;
using Cinema_Project.ViewModel;
using CinemaProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace Cinema_Project.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    [Authorize(Roles =$"{RolesName.SUPER_ADMIN},{RolesName.ADMIN},{RolesName.EMPLOYEE}")]
    public class ShowTimeController : Controller
    {
        private readonly IRepository<Cinema> _cinemaRepository;
        private readonly IRepository<Movie> _movieRepository;
        private readonly IRepository<ShowTime> _showTimeRepository;

        public ShowTimeController(IRepository<Cinema> cinemaRepository,IRepository<Movie> movieRepository, IRepository<ShowTime> showTimeRepository)
        {
            _cinemaRepository = cinemaRepository;
            _movieRepository = movieRepository;
            _showTimeRepository = showTimeRepository;
        }
        public async Task<IActionResult> Index(string? query)
        {
            var showTimes = await _showTimeRepository.GetAsync(includes: [c => c.Cinema, m => m.Movie], Tracking: false);

            if (!string.IsNullOrEmpty(query))
            {
                showTimes = showTimes.Where(c => c.Movie.Name.ToLower().Contains(query.ToLower()));
            }
            ViewBag.query = query;

            return View(showTimes);
        }
        [HttpGet]
        [Authorize(Roles = $"{RolesName.ADMIN} ,{RolesName.SUPER_ADMIN}")]
        public async Task<IActionResult> Create()
        {
            ShowTimeVM showTimeVM = new ShowTimeVM();

            //var movies = await _movieRepository.GetAsync(Tracking: false);
            //var cinemas = await _cinemaRepository.GetAsync(Tracking: false);

            //showTimeVM.Movies = movies.Select(m => new SelectListItem
            //{
            //    Text = m.Name,
            //    Value = m.Id.ToString()
            //});

            //showTimeVM.Cinemas = cinemas.Select(c => new SelectListItem
            //{
            //    Text = c.Name,
            //    Value = c.Id.ToString()
            //});
            await SelectList(showTimeVM);
            return View(showTimeVM);
        }
        [HttpPost]

        public async Task<IActionResult> Create(ShowTimeVM showTimeVM)
        {
            await SelectList(showTimeVM);

            if (showTimeVM.StartTime <= DateTime.Now)
            {
                ModelState.AddModelError(nameof(ShowTimeVM.StartTime),
                    "Start time must be in the future.");
            }

            if (!ModelState.IsValid) return View(showTimeVM);

            ShowTime showTime = new ShowTime()
            {
                Id=showTimeVM.Id,
                CinemaId = showTimeVM.CinemaId,
                MovieId = showTimeVM.MovieId,
                StartTime = showTimeVM.StartTime,
                Price = showTimeVM.Price,
            };

            await _showTimeRepository.CreateAsync(showTime);
            await _showTimeRepository.Commit();

            TempData["success_notification"] = "Show time created successfully";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        [Authorize(Roles = $"{RolesName.ADMIN} ,{RolesName.SUPER_ADMIN}")]
        public async Task<IActionResult> Update(int id)
        {
            var showTime =await _showTimeRepository.GetOneAsync(e => e.Id == id);
            if (showTime == null) return NotFound();

            var showTimeVM = new ShowTimeVM()
            {
                Id = showTime.Id,
                CinemaId=showTime.CinemaId,
                MovieId = showTime.MovieId,
                Price=showTime.Price,
                StartTime=showTime.StartTime,
            };
            //var movies = await _movieRepository.GetAsync(Tracking: false);
            //var cinemas = await _cinemaRepository.GetAsync(Tracking: false);
            //showTimeVM.Movies = movies.Select(e => new SelectListItem
            //{
            //    Text = e.Name,
            //    Value = e.Id.ToString()
            //});
            //showTimeVM.Cinemas = cinemas.Select(e => new SelectListItem
            //{
            //    Text = e.Name,
            //    Value = e.Id.ToString()
            //});
            await SelectList(showTimeVM);
            return View(showTimeVM);
        }
        [HttpPost]
        public async Task<IActionResult> Update(ShowTimeVM showTimeVM)
        {
            await SelectList(showTimeVM);

            if (showTimeVM.StartTime <= DateTime.Now)
            {
                ModelState.AddModelError(nameof(ShowTimeVM.StartTime),
                    "Start time must be in the future.");
            }

            if (!ModelState.IsValid) return View(showTimeVM);


            var showTime = await _showTimeRepository.GetOneAsync(e => e.Id == showTimeVM.Id);
            if (showTime == null) return NotFound();

            showTime.StartTime = showTimeVM.StartTime;
            showTime.Price = showTimeVM.Price;
            showTime.MovieId = showTimeVM.MovieId;
            showTime.CinemaId =showTimeVM.CinemaId;

            _showTimeRepository.Update(showTime);
            await _showTimeRepository.Commit();
            TempData["success_notification"] = "Show time updated successfully";

            return RedirectToAction(nameof(Index));

        }

        [Authorize(Roles = $"{RolesName.ADMIN} ,{RolesName.SUPER_ADMIN}")]
        public async Task<IActionResult> Delete(int id)
        {
            var showTime = await _showTimeRepository.GetOneAsync(e => e.Id == id);
            if (showTime == null) return NotFound();

            _showTimeRepository.Delete(showTime);
            await _showTimeRepository.Commit();

            TempData["success_notification"] = "Show time deleted successfully";
            return RedirectToAction(nameof(Index));
        }

        private async Task SelectList(ShowTimeVM showTimeVM)
        {
            var movies = await _movieRepository.GetAsync(e=>e.Status== "Available" || e.Id == showTimeVM.MovieId,   Tracking: false);
            var cinemas = await _cinemaRepository.GetAsync(Tracking: false);
            showTimeVM.Movies = movies.Select(e => new SelectListItem
            {
                Text = e.Name,
                Value = e.Id.ToString()
            });
            showTimeVM.Cinemas = cinemas.Select(e => new SelectListItem
            {
                Text = e.Name,
                Value = e.Id.ToString()
            });
        }


    }
}
