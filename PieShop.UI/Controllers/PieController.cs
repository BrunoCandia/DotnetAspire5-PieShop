using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.OutputCaching;
using PieShop.BusinessLogic;
using PieShop.Models.Pie;
using PieShop.UI.Models;

namespace PieShop.UI.Controllers
{
    public class PieController : Controller
    {
        private readonly IPieService _pieService;
        private readonly ICategoryService _categoryService;
        private readonly int pageSize = 5;

        public PieController(ICategoryService categoryService, IPieService pieService)
        {
            _categoryService = categoryService;
            _pieService = pieService;
        }

        [OutputCache(PolicyName = "PieList")]
        public async Task<ViewResult> List(string category)
        {
            // Test OutputCache
            await Task.Delay(TimeSpan.FromSeconds(5));

            IEnumerable<Pie> pies;
            string? currentCategory;

            if (string.IsNullOrWhiteSpace(category))
            {
                pies = await _pieService.GetAllPiesAsync();
                currentCategory = "All pies";
            }
            else
            {
                pies = await _pieService.GetAllPiesAsync();
                pies = pies.Where(p => p.Category.Name == category).OrderBy(p => p.PieId); //TODO: create a method to do this!!!

                var categories = await _categoryService.GetAllCategoriesAsync(); //TODO: create a method to get the current category!!!
                currentCategory = categories.FirstOrDefault(c => c.Name == category)?.Name;
            }

            return View(new PieListViewModel(pies, currentCategory));
        }

        [OutputCache(PolicyName = "PieDetail")]
        public async Task<ActionResult> Detail(Guid pieId)
        {
            // Test OutputCache
            await Task.Delay(TimeSpan.FromSeconds(5));

            if (pieId == Guid.Empty)
            {
                return BadRequest();
            }

            var pie = await _pieService.GetPieByPieIdAsync(pieId);

            if (pie is null)
            {
                return NotFound();
            }

            var pieDetailViewModel = new PieDetailViewModel(pie);

            return View(pieDetailViewModel);
        }

        ////Base Course
        ////public async Task<IActionResult> Search(string searchQuery)
        ////{
        ////    IEnumerable<Pie> pies = Enumerable.Empty<Pie>();

        ////    if (string.IsNullOrWhiteSpace(searchQuery))
        ////    {
        ////        ViewData["ErrorMessage"] = "Please enter a search term.";
        ////        ViewData["SearchPerformed"] = false;

        ////        var emptyPieSearchViewModel = new PieSearchViewModel(pies);

        ////        return View(emptyPieSearchViewModel);
        ////    }

        ////    ViewData["CurrentFilter"] = searchQuery;
        ////    ViewData["SearchPerformed"] = true;

        ////    pies = await _pieService.SearchPiesAsync(searchQuery);

        ////    var pieSearchViewModel = new PieSearchViewModel(pies);

        ////    return View(pieSearchViewModel);
        ////}

        public async Task<IActionResult> Paginated(string orderBy, bool orderByDescending, int pageNumber)
        {
            ViewData["CurrentSort"] = orderBy;

            // TODO: review
            ////ViewData["NameSortParam"] = orderBy == "name" ? "name_desc" : "name";
            ////ViewData["PriceSortParam"] = orderBy == "price" ? "price_desc" : "price";

            if (pageNumber == 0) pageNumber = 1;

            var pies = await _pieService.GetPiesPaginatedAsync(orderBy, orderByDescending, pageNumber, pageSize);

            return View(pies);
        }

        public async Task<IActionResult> Search(string? searchQuery, string? searchCategory)
        {
            var allCategories = await _categoryService.GetAllCategoriesAsync();

            IEnumerable<SelectListItem> selectListItems = new SelectList(allCategories, "CategoryId", "Name", null);

            PieSearchViewModel pieSearchViewModel = null;

            if (searchQuery != null)
            {
                var pies = await _pieService.SearchPiesAsync(searchQuery, searchCategory);

                pieSearchViewModel = new PieSearchViewModel(pies, selectListItems, searchQuery, searchCategory);

                return View(pieSearchViewModel);
            }

            pieSearchViewModel = new PieSearchViewModel(new List<Pie>(), selectListItems, string.Empty, null);

            return View(pieSearchViewModel);
        }

        public async Task<IActionResult> FullDetail(Guid pieId)
        {
            if (pieId == Guid.Empty)
            {
                return BadRequest();
            }

            var pie = await _pieService.GetFullPieByPieIdAsync(pieId);

            if (pie is null)
            {
                return NotFound();
            }

            var pieDetailViewModel = new PieDetailViewModel(pie);

            return View(pieDetailViewModel);
        }

        public async Task<IActionResult> Add()
        {
            try
            {
                var allCategories = await _categoryService.GetAllCategoriesAsync();

                IEnumerable<SelectListItem> selectListItems = new SelectList(allCategories, "CategoryId", "Name", null);

                PieAddViewModel pieAddViewModel = new() { Categories = selectListItems };

                return View(pieAddViewModel);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"There was an error: {ex.Message}";
            }

            return View(new PieAddViewModel());

        }

        [HttpPost]
        public async Task<IActionResult> Add(PieAddViewModel pieAddViewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Pie pie = new()
                    {
                        CategoryId = pieAddViewModel.Pie.CategoryId,
                        ShortDescription = pieAddViewModel.Pie.ShortDescription,
                        LongDescription = pieAddViewModel.Pie.LongDescription,
                        Price = pieAddViewModel.Pie.Price,
                        AllergyInformation = pieAddViewModel.Pie.AllergyInformation,
                        ImageThumbnailUrl = pieAddViewModel.Pie.ImageThumbnailUrl,
                        ImageUrl = pieAddViewModel.Pie.ImageUrl,
                        IsInStock = pieAddViewModel.Pie.IsInStock,
                        IsPieOfTheWeek = pieAddViewModel.Pie.IsPieOfTheWeek,
                        Name = pieAddViewModel.Pie.Name
                    };

                    await _pieService.AddPieAsync(pie);
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Adding the pie failed, please try again! Error: {ex.Message}");
            }

            var allCategories = await _categoryService.GetAllCategoriesAsync();

            IEnumerable<SelectListItem> selectListItems = new SelectList(allCategories, "CategoryId", "Name", null);

            pieAddViewModel.Categories = selectListItems;

            return View(pieAddViewModel);
        }

        public async Task<IActionResult> Edit(Guid pieId)
        {
            if (pieId == Guid.Empty)
            {
                return BadRequest();
            }

            var allCategories = await _categoryService.GetAllCategoriesAsync();

            IEnumerable<SelectListItem> selectListItems = new SelectList(allCategories, "CategoryId", "Name", null);

            var selectedPie = await _pieService.GetPieByPieIdAsync(pieId);

            PieEditViewModel pieEditViewModel = new() { Categories = selectListItems, Pie = selectedPie };

            return View(pieEditViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Pie pie)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _pieService.UpdatePieAsync(pie);

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

            var allCategories = await _categoryService.GetAllCategoriesAsync();

            IEnumerable<SelectListItem> selectListItems = new SelectList(allCategories, "CategoryId", "Name", null);

            PieEditViewModel pieEditViewModel = new() { Categories = selectListItems, Pie = pie };

            return View(pieEditViewModel);
        }

        public async Task<IActionResult> Delete(Guid pieId)
        {
            var selectedCategory = await _pieService.GetPieByPieIdAsync(pieId);

            return View(selectedCategory);
        }

        [HttpPost]
        public async Task<IActionResult> PostDelete(Guid pieId)
        {
            if (pieId == Guid.Empty)
            {
                ViewData["ErrorMessage"] = "Deleting the pie failed, invalid ID!";
                return View();
            }

            try
            {
                await _pieService.DeletePieAsync(pieId);

                TempData["PieDeleted"] = "Pie deleted successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Deleting the pie failed, please try again! Error: {ex.Message}";
            }

            var selectedPie = await _pieService.GetPieByPieIdAsync(pieId);

            return View(selectedPie);
        }
    }
}
