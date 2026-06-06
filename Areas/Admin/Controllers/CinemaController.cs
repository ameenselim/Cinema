using Cinema_Project.Models;
using Cinema_Project.Repositories;
using Cinema_Project.Services;
using Cinema_Project.Utilities;
using Cinema_Project.ViewModel;
using CinemaProject.Data;
using CinemaProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Cinema_Project.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    [Authorize(Roles = $"{RolesName.ADMIN} ,{RolesName.SUPER_ADMIN} ,{RolesName.EMPLOYEE}")]
    public class CinemaController : Controller
    {
        //private ApplicationDbContext _context = new ApplicationDbContext();

        private CinemaServices _cinemaService;
        private IRepository<Cinema> _cinemaRepository;
        private readonly IRepository<Seat> _seatRepository;

        public CinemaController(IRepository<Cinema> cinemaRepository ,IRepository<Seat> seatRepository)
        {
            _cinemaRepository = cinemaRepository;
            _seatRepository = seatRepository;
            _cinemaService = new CinemaServices();
        }

        public async Task<IActionResult> Index(string? query = null, int page = 1)
        {
            //var cinemas = _context.Cinemas.AsQueryable();
            var cinemas = await _cinemaRepository.GetAsync();

            if (!string.IsNullOrEmpty(query))
            {
                cinemas = cinemas.Where(c => c.Name.Contains(query));
            }
            //pagination
            int totalpige = (int)Math.Ceiling(cinemas.Count() / 4.0);

            cinemas = cinemas.Skip((page - 1) * 4).Take(4);


            return View(new CinemaWithRelated()
            {
                Query = query,
                TotalPage = totalpige,
                CurrentPage = page,
                cinemas = cinemas.AsEnumerable()
            });
        }
        [HttpGet]
        [Authorize(Roles = $"{RolesName.ADMIN} ,{RolesName.SUPER_ADMIN}")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCinemaVM createCinemaVM, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(createCinemaVM);

            if (createCinemaVM.CinemaImg is not null && createCinemaVM.CinemaImg.Length > 0)
            {
                var fileName = _cinemaService.SaveImg(createCinemaVM.CinemaImg);

                if (fileName is null)
                {
                    ModelState.AddModelError("CinemaImg", "Image upload failed");
                    return View(createCinemaVM);
                }

                createCinemaVM.Logo = fileName;
            }

            Cinema cinema = new Cinema()
            {
                Name = createCinemaVM.Name,
                Description = createCinemaVM.Description,
                Address = createCinemaVM.Address,
                Logo = createCinemaVM.Logo!,
                RowsCount = createCinemaVM.RowsCount,
                SeatsPerRow = createCinemaVM.SeatsPerRow,
            };

            await _cinemaRepository.CreateAsync(cinema, cancellationToken);
            await _cinemaRepository.Commit();

            TempData["success_notification"] = "Cinema created successfully!";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        [Authorize(Roles = $"{RolesName.ADMIN} ,{RolesName.SUPER_ADMIN}")]
        public async Task<IActionResult> Update(int id)
        {
            var cinema = await _cinemaRepository.GetOneAsync(c => c.Id == id);

            if (cinema == null)
                return NotFound();

            UpdateCinemaVM updateCinemaVM = new UpdateCinemaVM()
            {
                Id = cinema.Id,
                Name = cinema.Name,
                Description = cinema.Description,
                Address = cinema.Address,
                Logo = cinema.Logo,
                RowsCount = cinema.RowsCount,
                SeatsPerRow = cinema.SeatsPerRow,
            };

            return View(updateCinemaVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateCinemaVM updateCinemaVM)
        {
            if (!ModelState.IsValid)
                return View(updateCinemaVM);

            var cinemainDb = await _cinemaRepository.GetOneAsync(e => e.Id == updateCinemaVM.Id);

            if (cinemainDb is null)
                return NotFound();

            if (updateCinemaVM.CinemaImg is not null && updateCinemaVM.CinemaImg.Length > 0)
            {
                var fileName = _cinemaService.SaveImg(updateCinemaVM.CinemaImg);

                if (fileName is null)
                {
                    ModelState.AddModelError("CinemaImg", "Image upload failed");
                    return View(updateCinemaVM);
                }

                _cinemaService.DeleteImg(cinemainDb.Logo);
                cinemainDb.Logo = fileName;
            }

            cinemainDb.Name = updateCinemaVM.Name;
            cinemainDb.Description = updateCinemaVM.Description;
            cinemainDb.Address = updateCinemaVM.Address;
            cinemainDb.RowsCount = updateCinemaVM.RowsCount;
            cinemainDb.SeatsPerRow = updateCinemaVM.SeatsPerRow;

            _cinemaRepository.Update(cinemainDb);
            await _cinemaRepository.Commit();
            await UpdateCinemaSeats(cinemainDb);

            TempData["success_notification"] = "Cinema updated successfully!";

            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = $"{RolesName.ADMIN} ,{RolesName.SUPER_ADMIN}")]
        public async Task<IActionResult> Delete(int id)
        {
            //var cinema = _context.Cinemas.SingleOrDefault(c => c.Id == id);
            var cinema = await _cinemaRepository.GetOneAsync(c => c.Id == id);
            if (cinema == null) return NotFound();

            _cinemaService.DeleteImg(cinema.Logo);

            //_context.Cinemas.Remove(cinema);
            //_context.SaveChanges();
            _cinemaRepository.Delete(cinema);
            await _cinemaRepository.Commit();
            TempData["success_notification"] = "Cinema deleted successfully!";
            return RedirectToAction(nameof(Index));
        }


        private async Task UpdateCinemaSeats(Cinema cinema)
        {
            var existingSeats = await _seatRepository.GetAsync(s => s.CinemaId == cinema.Id);

            var existingSeatNumbers = existingSeats
                .Select(s => s.SeatNumber)
                .ToList();

            for (int row = 0; row < cinema.RowsCount; row++)
            {
                char rowLetter = (char)('A' + row);

                for (int n = 1; n <= cinema.SeatsPerRow; n++)
                {
                    string seatNumber = $"{rowLetter}{n}";

                    if (!existingSeatNumbers.Contains(seatNumber))
                    {
                        await _seatRepository.CreateAsync(new Seat
                        {
                            CinemaId = cinema.Id,
                            SeatNumber = seatNumber
                        });
                    }
                }
            }

            await _seatRepository.Commit();
        }
    }
}

