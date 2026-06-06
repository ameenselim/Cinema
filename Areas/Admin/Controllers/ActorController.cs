
using Cinema_Project.Repositories;
using Cinema_Project.Services;
using Cinema_Project.ViewModel;
using Cinema_Project.Utilities;
using CinemaProject.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;

namespace Cinema_Project.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    [Authorize(Roles = $"{RolesName.ADMIN} ,{RolesName.SUPER_ADMIN} ,{RolesName.EMPLOYEE}")]
    public class ActorController : Controller
    {
        private readonly IRepository<Actor> _actorRepository;
        private readonly IRepository<Movie> _movieRepository;
        private readonly ActorServices _actorServices = new();

        public ActorController(
            IRepository<Actor> actorRepository,
            IRepository<Movie> movieRepository)
        {
            _actorRepository = actorRepository;
            _movieRepository = movieRepository;
        }

        public async Task<IActionResult> Index(string? query = null, int page = 1)
        {
            var actors = await _actorRepository.GetAsync(Tracking: false);

            if (!string.IsNullOrEmpty(query))
            {
                actors = actors.Where(c => c.Name.Contains(query));
            }

            // pagination
            int totalpige = (int)Math.Ceiling(actors.Count() / 4.0);

            actors = actors.Skip((page - 1) * 4).Take(4);

            return View(new ActorWithRelated()
            {
                Query = query,
                TotalPage = totalpige,
                CurrentPage = page,
                actors = actors.AsEnumerable()
            });
        }

        [HttpGet]
        [Authorize(Roles = $"{RolesName.ADMIN} ,{RolesName.SUPER_ADMIN}")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        
        public async Task<IActionResult> Create(CreateActorVM createActorVM)
        {
            if (!ModelState.IsValid)
                return View(createActorVM);

            if (createActorVM.actorImg is not null && createActorVM.actorImg.Length > 0)
            {
                var fileName = _actorServices.SaveImg(createActorVM.actorImg);

                if (fileName is  null)
                {
                    ModelState.AddModelError("actorImg", "Image upload failed");
                    return View(createActorVM);
                }
                createActorVM.Image = fileName;
            }
            Actor actor = new Actor()
            {
                Name = createActorVM.Name,
                Image = createActorVM.Image!
            };

            await _actorRepository.CreateAsync(actor);
            await _actorRepository.Commit();
            TempData["success_notification"] = "Actor created successfully";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        [Authorize(Roles = $"{RolesName.ADMIN} ,{RolesName.SUPER_ADMIN}")]
        public async Task<IActionResult> Update(int id)
        {
            var actor = await _actorRepository.GetOneAsync(c => c.Id == id);

            if (actor == null)
            {
                return NotFound();
            }
            UpdateActorVM updateActorVM = new()
            {
                Id = actor.Id,
                Name = actor.Name,
                Image = actor.Image,
            };


            return View(updateActorVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateActorVM updateActorVM)
        {
            ModelState.Remove(nameof(updateActorVM.actorImg));

            if (!ModelState.IsValid)
                return View(updateActorVM);

            var actor = await _actorRepository.GetOneAsync(
                e => e.Id == updateActorVM.Id);

            if (actor is null)  return NotFound();

            if (updateActorVM.actorImg is not null && updateActorVM.actorImg.Length > 0)
            {
                var fileName = _actorServices.SaveImg(updateActorVM.actorImg);

                if (fileName is null)
                {
                    ModelState.AddModelError("actorImg", "Image upload failed");
                    return View(updateActorVM);
                }

                _actorServices.DeleteImg(actor.Image);

                actor.Image = fileName;
            }
            actor.Name = updateActorVM.Name;
            
            _actorRepository.Update(actor);
            await _actorRepository.Commit();

            TempData["success_notification"] = "Actor updated successfully";

            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = $"{RolesName.ADMIN} ,{RolesName.SUPER_ADMIN}")]
        public async Task<IActionResult> Delete(int id)
        {
            var actor = await _actorRepository.GetOneAsync(c => c.Id == id);

            if (actor == null)
            {
                return NotFound();
            }

            _actorServices.DeleteImg(actor.Image);

            _actorRepository.Delete(actor);
            await _actorRepository.Commit();

            TempData["success_notification"] = "Actor deleted successfully";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        [Authorize(Roles = $"{RolesName.ADMIN} ,{RolesName.SUPER_ADMIN}")]
        public async Task<IActionResult> Assign(int id)
        {
            var actor = await _actorRepository.GetOneAsync(
                c => c.Id == id,
                includes: new Expression<Func<Actor, object>>?[] { c => c.Movies }
            );

            if (actor is null)
            {
                return NotFound();
            }

            ViewBag.Movies = await _movieRepository.GetAsync(Tracking: false);

            return View(actor);
        }

        [HttpPost]
        public async Task<IActionResult> Assign(int id, List<int>? moviesIds)
        {
            var actor = await _actorRepository.GetOneAsync(
                c => c.Id == id,
                includes: [e => e.Movies]);

            if (actor is null)  return NotFound();
            moviesIds ??= new List<int>();

            moviesIds = moviesIds.Distinct().ToList();

            var selectedMovies = await _movieRepository.GetAsync(e => moviesIds.Contains(e.Id));

            var existMovies = actor.Movies.Select(e => e.Id);

            actor.Movies.Clear();

            foreach (var item in selectedMovies)
            {
                if(!existMovies.Contains(item.Id))
                {
                    actor.Movies.Add(item);
                }
            }
            await _actorRepository.Commit();

            TempData["success_notification"] = "Movies assigned to actor successfully";

            return RedirectToAction(nameof(Index));

        }
    }
}
