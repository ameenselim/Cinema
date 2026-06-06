using Cinema_Project.Models;
using Cinema_Project.Utilities;
using Cinema_Project.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Cinema_Project.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    [Authorize(Roles = $"{RolesName.SUPER_ADMIN}")]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }
        public async Task<IActionResult> Index(string? query, int page = 1)
        {
            var roles = _roleManager.Roles.AsQueryable();

            if (roles is null) return NotFound();

            if (!string.IsNullOrEmpty(query))
            {
                roles = roles.Where(e => e.Name!.Contains(query));
            }

            int totalpages = (int)Math.Ceiling(roles.Count() / 5.0);

            roles = roles.Skip((page - 1) * 5).Take(5);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalpages;
            ViewBag.Query = query;

            return View(roles);
        }
        [HttpGet]
        public IActionResult Create()
        {

            return View(new CreateRoleVM());

        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateRoleVM createRoleVM)
        {
            if (!ModelState.IsValid) return View(createRoleVM);

            IdentityRole role = new IdentityRole()
            {
                Name = createRoleVM.Name
            };
            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError(item.Code, item.Description);
                }
                return View(createRoleVM);
            }
            TempData["success_notification"] = "Role Created successfully";
            return RedirectToAction(nameof(Index));

        }
        [HttpGet]
        public async Task<IActionResult> Update(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null) return NotFound();

            UpdateRoleVM updateRoleVM = new UpdateRoleVM()
            {
                Id = role.Id,
                Name = role.Name!
            };


            return View(updateRoleVM);
        }
        [HttpPost]
        public async Task<IActionResult> Update(UpdateRoleVM updateRoleVM)
        {
            if (!ModelState.IsValid) return View(updateRoleVM);

            var role = await _roleManager.FindByIdAsync(updateRoleVM.Id);

            if (role == null) return NotFound();

            role.Name = updateRoleVM.Name;
            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError(item.Code, item.Description);
                }
                return View(updateRoleVM);
            }
            TempData["success_notification"] = "Role updated successfully";

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            await _roleManager.DeleteAsync(role);
            TempData["success_notification"] = "Role Deleted successfully";

            return RedirectToAction(nameof(Index));
        }

    }

}
