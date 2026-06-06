using Cinema_Project.Repositories;
using Cinema_Project.Services;
using Cinema_Project.Utilities;
using Cinema_Project.ViewModel;
using CinemaProject.Models;
using Ecomerce.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace Cinema_Project.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    [Authorize(Roles = $"{RolesName.ADMIN} ,{RolesName.SUPER_ADMIN}, {RolesName.EMPLOYEE}")]
    public class MovieController : Controller
    {
        private readonly IRepository<Movie> _movieRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Cinema> _cinemaRepository;
        private readonly IRepository<MovieSubImage> _movieSubImageRepository;
        private readonly IRepository<Actor> _actorRepository;

        private MovieServices _movieServices = new();

        public MovieController(
            IRepository<Movie> movieRepository,
            IRepository<Category> categoryRepository,
            IRepository<Cinema> cinemaRepository,
            IRepository<MovieSubImage> movieSubImageRepository,
            IRepository<Actor> actorRepository)
        {
            _movieRepository = movieRepository;
            _categoryRepository = categoryRepository;
            _cinemaRepository = cinemaRepository;
            _movieSubImageRepository = movieSubImageRepository;
            _actorRepository = actorRepository;
        }

        public async Task<IActionResult> Index(string? query = null, int page = 1)
        {
            var movies = await _movieRepository.GetAsync(
                includes: [e => e.Category, e => e.Cinema]
            );

            if (!string.IsNullOrEmpty(query))
            {
                movies = movies.Where(c => c.Name.Contains(query));
            }

            //pagination
            int totalpige = (int)Math.Ceiling(movies.Count() / 4.0);

            movies = movies.Skip((page - 1) * 4).Take(4);

            return View(new MovieWithRelated()
            {
                Query = query,
                TotalPage = totalpige,
                CurrentPage = page,
                movies = movies.AsEnumerable()
            });
        }

        [HttpGet]
        [Authorize(Roles = $"{RolesName.ADMIN} ,{RolesName.SUPER_ADMIN}")]
        public async Task<IActionResult> Create()
        {
            return View(new CreateMovieVM()
            {
                Categories = await _categoryRepository.GetAsync(Tracking: false),
                Cinemas = await _cinemaRepository.GetAsync(Tracking: false)
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateMovieVM createMovieVM)
        {
            if (!ModelState.IsValid)
            {
                createMovieVM.Categories = await _categoryRepository.GetAsync(Tracking: false);
                createMovieVM.Cinemas = await _cinemaRepository.GetAsync(Tracking: false);

                return View(createMovieVM);
            }

            var movie = new Movie()
            {
                Name = createMovieVM.Name,
                Description = createMovieVM.Description,
                Price = (decimal)createMovieVM.Price,
                Date = createMovieVM.Date,
                Status = createMovieVM.Status,
                CategoryId = createMovieVM.CategoryId,
                CinemaId = createMovieVM.CinemaId
            };

            // Main Image
            if (createMovieVM.MainImgFile is not null && createMovieVM.MainImgFile.Length > 0)
            {
                var fileName = _movieServices.SavImg(createMovieVM.MainImgFile, ProductImgType.MainImg);

                if (fileName is null)
                {
                    ModelState.AddModelError("MainImgFile", "Main image upload failed");

                    createMovieVM.Categories = await _categoryRepository.GetAsync(Tracking: false);
                    createMovieVM.Cinemas = await _cinemaRepository.GetAsync(Tracking: false);

                    return View(createMovieVM);
                }

                movie.MainImg = fileName;
            }

            await _movieRepository.CreateAsync(movie);
            await _movieRepository.Commit();

            // Sub Images
            if (createMovieVM.SubImgs is not null && createMovieVM.SubImgs.Any())
            {
                foreach (var item in createMovieVM.SubImgs)
                {
                    if (item is not null && item.Length > 0)
                    {
                        var fileName = _movieServices.SavImg(item, ProductImgType.SubImg);

                        if (fileName is not null)
                        {
                            await _movieSubImageRepository.CreateAsync(new MovieSubImage()
                            {
                                SubImg = fileName,
                                MovieId = movie.Id
                            });
                        }
                    }
                }

                await _movieSubImageRepository.Commit();
            }

            TempData["success_notification"] = "Movie created successfully";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        [Authorize(Roles = $"{RolesName.ADMIN} ,{RolesName.SUPER_ADMIN}")]
        public async Task<IActionResult> Update(int id)
        {
            var movie = await _movieRepository.GetOneAsync(
                e => e.Id == id,
                includes: [e => e.Category, e => e.Cinema]
            );

            if (movie is null)
                return NotFound();

            UpdateMovieVM updateMovieVM = new UpdateMovieVM()
            {
                Id = movie.Id,
                Name = movie.Name,
                Description = movie.Description,
                Price = (double)movie.Price,
                Date = movie.Date,
                Status = movie.Status,
                CategoryId = movie.CategoryId,
                CinemaId = movie.CinemaId,
                MainImg = movie.MainImg,

                Categories = await _categoryRepository.GetAsync(Tracking: false),
                Cinemas = await _cinemaRepository.GetAsync(Tracking: false),
                MovieSubImgs = (await _movieSubImageRepository.GetAsync(e => e.MovieId == id)).ToList()
            };

            return View(updateMovieVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateMovieVM updateMovieVM)
        {
            if (!ModelState.IsValid)
            {
                updateMovieVM.Categories = await _categoryRepository.GetAsync(Tracking: false);
                updateMovieVM.Cinemas = await _cinemaRepository.GetAsync(Tracking: false);
                updateMovieVM.MovieSubImgs = (await _movieSubImageRepository.GetAsync(e => e.MovieId == updateMovieVM.Id)).ToList();

                return View(updateMovieVM);
            }

            var movie = await _movieRepository.GetOneAsync(e => e.Id == updateMovieVM.Id);

            if (movie is null)
                return NotFound();

            if (updateMovieVM.MainImgFile is not null && updateMovieVM.MainImgFile.Length > 0)
            {
                var fileName = _movieServices.SavImg(updateMovieVM.MainImgFile, ProductImgType.MainImg);

                if (fileName is null)
                {
                    ModelState.AddModelError("MainImgFile", "Main image upload failed");

                    updateMovieVM.Categories = await _categoryRepository.GetAsync(Tracking: false);
                    updateMovieVM.Cinemas = await _cinemaRepository.GetAsync(Tracking: false);
                    updateMovieVM.MovieSubImgs = (await _movieSubImageRepository.GetAsync(e => e.MovieId == updateMovieVM.Id)).ToList();

                    return View(updateMovieVM);
                }

                _movieServices.DeleteImg(movie.MainImg, ProductImgType.MainImg);

                movie.MainImg = fileName;
            }

            movie.Name = updateMovieVM.Name;
            movie.Description = updateMovieVM.Description;
            movie.Price = (decimal)updateMovieVM.Price;
            movie.Date = updateMovieVM.Date;
            movie.Status = updateMovieVM.Status;
            movie.CategoryId = updateMovieVM.CategoryId;
            movie.CinemaId = updateMovieVM.CinemaId;

            _movieRepository.Update(movie);
            await _movieRepository.Commit();

            if (updateMovieVM.SubImgs is not null && updateMovieVM.SubImgs.Any())
            {
                var movieSubImgs = (await _movieSubImageRepository.GetAsync(e => e.MovieId == movie.Id)).ToList();

                foreach (var i in movieSubImgs)
                {
                    _movieServices.DeleteImg(i.SubImg, ProductImgType.SubImg);
                }

                foreach (var item in movieSubImgs)
                {
                    _movieSubImageRepository.Delete(item);
                }

                foreach (var item in updateMovieVM.SubImgs)
                {
                    if (item is not null && item.Length > 0)
                    {
                        var fileName = _movieServices.SavImg(item, ProductImgType.SubImg);

                        if (fileName is not null)
                        {
                            await _movieSubImageRepository.CreateAsync(new MovieSubImage()
                            {
                                SubImg = fileName,
                                MovieId = movie.Id
                            });
                        }
                    }
                }

                await _movieSubImageRepository.Commit();
            }

            TempData["success_notification"] = "Movie updated successfully";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = $"{RolesName.ADMIN} ,{RolesName.SUPER_ADMIN}")]
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _movieRepository.GetOneAsync(e => e.Id == id);

            if (movie is null) return NotFound();

            var movieSubImages = await _movieSubImageRepository.GetAsync(e => e.MovieId == id);

            foreach (var item in movieSubImages)
            {
                _movieServices.DeleteImg(item.SubImg, ProductImgType.SubImg);
                _movieSubImageRepository.Delete(item);
            }

            _movieServices.DeleteImg(movie.MainImg, ProductImgType.MainImg);

            _movieRepository.Delete(movie);

            await _movieRepository.Commit();
            TempData["success_notification"] = "Movie deleted successfully";

            return RedirectToAction(nameof(Index));
        }

        // Assign actors to movie
        [HttpGet]
        [Authorize(Roles = $"{RolesName.ADMIN} ,{RolesName.SUPER_ADMIN}")]
        public async Task<IActionResult> Assign(int id)
        {
            var movie = await _movieRepository.GetOneAsync(
                m => m.Id == id,
                includes: [m => m.Actors]
            );

            if (movie is null) return NotFound();

            ViewBag.Actors = await _actorRepository.GetAsync();

            return View(movie);
        }

        [HttpPost]
        public async Task<IActionResult> Assign(int id, List<int>? actorIds)
        {
            var movie = await _movieRepository.GetOneAsync(
                c => c.Id == id,
                includes: [m => m.Actors]
            );

            if (movie is null) return NotFound();

            // if no actors assigned to this movie || the user unassigned all actors from this movie
            actorIds ??= new List<int>();
            actorIds = actorIds.Distinct().ToList();



            // get exist actors that assigned to this movie
            var existActorIds = movie.Actors.Select(e => e.Id).ToList();

            // get new actors that assigned to this movie
            var selectedActors = await _actorRepository.GetAsync(e => actorIds.Contains(e.Id));


            movie.Actors.Clear();

            foreach (var actor in selectedActors)
                movie.Actors.Add(actor);

            await _movieRepository.Commit();

            TempData["success_notification"] = "Actors assigned to movie successfully";

            return RedirectToAction(nameof(Index));
        }
    }
}