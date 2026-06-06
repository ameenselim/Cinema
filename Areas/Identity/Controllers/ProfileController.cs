using Cinema_Project.Models;
using Cinema_Project.Utilities;
using Cinema_Project.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Cinema_Project.Areas.Identity.Controllers
{
    [Area(SD.IDENTITY_AREA)]
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            ApplicationUserVM applicationUserVM = new()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                UserName = user.UserName!,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber
            };

            return View(applicationUserVM);
        }
        [HttpPost]
        public async Task<IActionResult> SaveProfile(ApplicationUserVM applicationUserVM)
        {
            if (!ModelState.IsValid) return View(nameof(Index), applicationUserVM);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FirstName = applicationUserVM.FirstName;
            user.LastName = applicationUserVM.LastName;
            user.Email = applicationUserVM.Email;
            user.UserName = applicationUserVM.UserName;
            user.Address = applicationUserVM.Address;
            user.PhoneNumber = applicationUserVM.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(nameof(Index), applicationUserVM);
            }

            TempData["success_notification"] = "Update User Info successfully";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordVM updatePasswordVM)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                var vm = new ApplicationUserVM
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email!,
                    UserName = user.UserName!,
                    Address = user.Address,
                    PhoneNumber = user.PhoneNumber
                };

                return View(nameof(Index), vm);
            }

            var result = await _userManager.ChangePasswordAsync(user, updatePasswordVM.CurrentPassword, updatePasswordVM.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                var vm = new ApplicationUserVM
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email!,
                    UserName = user.UserName!,
                    Address = user.Address,
                    PhoneNumber = user.PhoneNumber
                };

                return View(nameof(Index), vm);
            }

            TempData["success_notification"] = "Update Password successfully";

            return RedirectToAction(nameof(Index));
        }
    }
}
