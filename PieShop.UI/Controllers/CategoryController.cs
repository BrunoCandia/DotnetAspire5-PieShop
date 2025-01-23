using Microsoft.AspNetCore.Mvc;
using PieShop.BusinessLogic;
using PieShop.Models.Category;
using PieShop.UI.Models;

namespace PieShop.UI.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();

            var categoryListViewModel = new CategoryListViewModel { Categories = categories.ToList() };

            return View(categoryListViewModel);
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Category category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _categoryService.AddCategoryAsync(category);

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Adding the category failed, please try again! Error: {ex.Message}");
            }

            return View(category);
        }

        public async Task<IActionResult> Detail(Guid categoryId)
        {
            if (categoryId == Guid.Empty)
            {
                return BadRequest();
            }

            var category = await _categoryService.GetCategoryByCategoryIdAsync(categoryId);

            if (category is null)
            {
                return NotFound();
            }

            var pieDetailViewModel = new CategoryDetailViewModel(category);

            return View(pieDetailViewModel);
        }

        public async Task<IActionResult> Edit(Guid categoryId)
        {

            if (categoryId == Guid.Empty)
            {
                return BadRequest();
            }

            var selectedCategory = await _categoryService.GetCategoryByCategoryIdAsync(categoryId);

            return View(selectedCategory);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Category category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _categoryService.UpdateCategoryAsync(category);

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Updating the category failed, please try again! Error: {ex.Message}");
            }

            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid categoryId, bool? isdummy)
        {
            var selectedCategory = await _categoryService.GetCategoryByCategoryIdAsync(categoryId);

            return View(selectedCategory);
        }

        [HttpPost]
        public async Task<IActionResult> PostDelete(Guid categoryId)
        {
            if (categoryId == Guid.Empty)
            {
                ViewData["ErrorMessage"] = "Deleting the category failed, invalid ID!";
                return View("Delete");
            }

            try
            {
                await _categoryService.DeleteCategoryAsync(categoryId);

                TempData["CategoryDeleted"] = "Category deleted successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Deleting the category failed, please try again! Error: {ex.Message}";
            }

            var selectedCategory = await _categoryService.GetCategoryByCategoryIdAsync(categoryId);

            return View(selectedCategory);

        }
    }
}
