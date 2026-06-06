
using Cinema_Project.Repositories;
using Cinema_Project.Utilities;
using Cinema_Project.ViewModel;
using CinemaProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cinema_Project.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    [Authorize(Roles = $"{RolesName.ADMIN} ,{RolesName.SUPER_ADMIN} ,{RolesName.EMPLOYEE}")]
    public class CategoryController : Controller
    {
        private IRepository<Category> _categoryRepository;

        public CategoryController(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index(string? query = null, int page = 1)
        {
            var categorys = await _categoryRepository.GetAsync();

            if (!string.IsNullOrEmpty(query))
            {
                categorys = categorys.Where(c => c.Name.Contains(query));
            }

            //pagination
            int totalpige = (int)Math.Ceiling(categorys.Count() / 4.0);

            categorys = categorys.Skip((page - 1) * 4).Take(4);

            return View(new CategoryWithRelated()
            {
                Query = query,
                TotalPage = totalpige,
                CurrentPage = page,
                categories = categorys.AsEnumerable()
            });
        }

        [HttpGet]
        [Authorize(Roles = $"{RolesName.ADMIN} ,{RolesName.SUPER_ADMIN}")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryVM createCategory)
        {
            if (!ModelState.IsValid) return View(createCategory);
            var category = new Category
            {
                Name = createCategory.Name,
                Description = createCategory.Description
            };

            await _categoryRepository.CreateAsync(category);
            await _categoryRepository.Commit();

            TempData["success_notification"] = "Category created successfully!";
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        [Authorize(Roles = $"{RolesName.ADMIN} ,{RolesName.SUPER_ADMIN}")]
        public async Task<IActionResult> Update(int id)
        {
            var category = await _categoryRepository.GetOneAsync(c => c.Id == id);

            if (category == null) return NotFound();

            UpdateCategoryVM updateCategory = new UpdateCategoryVM
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            return View(updateCategory);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateCategoryVM updateCategory)
        {
            if (!ModelState.IsValid) return View(updateCategory);

            var category = await _categoryRepository.GetOneAsync(c => c.Id == updateCategory.Id);
            if (category == null) return NotFound();
            category.Name = updateCategory.Name!;
            category.Description = updateCategory.Description!;


            _categoryRepository.Update(category);
            await _categoryRepository.Commit();

            TempData["success_notification"] = "Category updated successfully!";

            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = $"{RolesName.ADMIN} ,{RolesName.SUPER_ADMIN}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetOneAsync(c => c.Id == id);

            if (category == null) return NotFound();

            _categoryRepository.Delete(category);
            await _categoryRepository.Commit();

            TempData["success_notification"] = "Category deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}