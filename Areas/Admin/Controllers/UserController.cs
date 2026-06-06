using Cinema_Project.Models;
using Cinema_Project.Utilities;
using Cinema_Project.ViewModel;
using CinemaProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Cinema_Project.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    [Authorize(Roles = $"{RolesName.SUPER_ADMIN},{RolesName.ADMIN}")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index(string query, int page = 1)
        {
            var users = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                users = users.Where(c => c.UserName!.Contains(query));
            }
            //pagination
            int totalpages = (int)Math.Ceiling(users.Count() / 5.0);

            users = users.Skip((page - 1) * 5).Take(5);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalpages;
            ViewBag.Query = query;

            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Roles = GetAllowedRoles();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserVM createUserVM)
        {
            ViewBag.Roles = GetAllowedRoles();

            if (!ModelState.IsValid)
                return View(createUserVM);

            if (!IsRoleAllowed(createUserVM.Role))
            {
                ModelState.AddModelError(nameof(createUserVM.Role), "You are not allowed to assign this role.");
                return View(createUserVM);
            }

            var user = new ApplicationUser
            {
                UserName = createUserVM.UserName,
                Email = createUserVM.Email,
                FirstName = createUserVM.FirstName,
                LastName = createUserVM.LastName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, createUserVM.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(createUserVM);
            }

            await _userManager.AddToRoleAsync(user, createUserVM.Role);

            TempData["success_notification"] = "User created successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Update(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var vm = new UpdateUserVM
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = roles.FirstOrDefault() ?? ""
            };

            ViewBag.Roles = GetAllowedRoles();

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateUserVM vm)
        {
            ViewBag.Roles = GetAllowedRoles();

            if (!ModelState.IsValid)
                return View(vm);

            if (!IsRoleAllowed(vm.Role))
            {
                ModelState.AddModelError(nameof(vm.Role), "You are not allowed to assign this role.");
                return View(vm);
            }

            var user = await _userManager.FindByIdAsync(vm.Id);

            if (user is null)
                return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Contains(RolesName.SUPER_ADMIN) && !User.IsInRole(RolesName.SUPER_ADMIN))
                return Forbid();

            user.UserName = vm.UserName;
            user.Email = vm.Email;
            user.FirstName = vm.FirstName;
            user.LastName = vm.LastName;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(vm);
            }

            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, vm.Role);

            TempData["success_notification"] = "User updated successfully";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Lock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains(RolesName.SUPER_ADMIN) && !User.IsInRole(RolesName.SUPER_ADMIN))
                return Forbid();

            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.Now.AddYears(10));

            TempData["success_notification"] = "User locked successfully";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Unlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
                return NotFound();

            await _userManager.SetLockoutEndDateAsync(user, null);

            TempData["success_notification"] = "User unlocked successfully";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains(RolesName.SUPER_ADMIN) && !User.IsInRole(RolesName.SUPER_ADMIN))
                return Forbid();

            await _userManager.DeleteAsync(user);
            TempData["success_notification"] = "User deleted successfully";
            return RedirectToAction(nameof(Index));
        }

        // Only Super Admin can see all roles, Admin can only see Employee and Customer roles
        private List<SelectListItem> GetAllowedRoles()
        {
            if (User.IsInRole(RolesName.SUPER_ADMIN))
            {
                return _roleManager.Roles
                    .Select(r => new SelectListItem
                    {
                        Text = r.Name,
                        Value = r.Name
                    })
                    .ToList();
            }

            return _roleManager.Roles
                .Where(r => r.Name == RolesName.EMPLOYEE || r.Name == RolesName.CUSTOMER)
                .Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                })
                .ToList();
        }

        // Check if the role is allowed to be assigned by the current user
        private bool IsRoleAllowed(string role)
        {
            if (User.IsInRole(RolesName.SUPER_ADMIN))
                return true;

            return role == RolesName.EMPLOYEE || role == RolesName.CUSTOMER;
        }
    }
}