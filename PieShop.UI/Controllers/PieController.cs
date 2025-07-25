﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
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
        private readonly ILogger<PieController> _logger;

        public PieController(ICategoryService categoryService, IPieService pieService, ILogger<PieController> logger)
        {
            _categoryService = categoryService;
            _pieService = pieService;
            _logger = logger;
        }

        [OutputCache(PolicyName = "PieList")]
        public async Task<ViewResult> List(string category)
        {
            // Test OutputCache
            await Task.Delay(TimeSpan.FromSeconds(5));

            _logger.LogInformation("5 seconds delayed for fetching pies for category");

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

            _logger.LogInformation("5 seconds delayed for fetching pie details");

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
            var pieToUpdate = await _pieService.GetPieByPieIdAsync(pie.PieId);

            try
            {
                if (pieToUpdate == null)
                {
                    ModelState.AddModelError(string.Empty, "The pie you want to update doesn't exist or was already deleted by someone else.");
                }

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
            catch (DbUpdateConcurrencyException ex)
            {
                var exceptionPie = ex.Entries.Single();
                var entityValues = (Pie)exceptionPie.Entity;
                var databasePie = await exceptionPie.GetDatabaseValuesAsync();

                if (databasePie == null)
                {
                    ModelState.AddModelError(string.Empty, "The pie was already deleted by another user.");
                }
                else
                {
                    var databaseValues = (Pie)databasePie.ToObject();

                    if (databaseValues.Name != entityValues.Name)
                    {
                        ModelState.AddModelError("Pie.Name", $"Current value: {databaseValues.Name}");
                    }
                    if (databaseValues.Price != entityValues.Price)
                    {
                        ModelState.AddModelError("Pie.Price", $"Current value: {databaseValues.Price:c}");
                    }
                    if (databaseValues.ShortDescription != entityValues.ShortDescription)
                    {
                        ModelState.AddModelError("Pie.ShortDescription", $"Current value: {databaseValues.ShortDescription}");
                    }
                    if (databaseValues.LongDescription != entityValues.LongDescription)
                    {
                        ModelState.AddModelError("Pie.LongDescription", $"Current value: {databaseValues.LongDescription}");
                    }
                    if (databaseValues.AllergyInformation != entityValues.AllergyInformation)
                    {
                        ModelState.AddModelError("Pie.AllergyInformation", $"Current value: {databaseValues.AllergyInformation}");
                    }
                    if (databaseValues.ImageThumbnailUrl != entityValues.ImageThumbnailUrl)
                    {
                        ModelState.AddModelError("Pie.ImageThumbnailUrl", $"Current value: {databaseValues.ImageThumbnailUrl}");
                    }
                    if (databaseValues.ImageUrl != entityValues.ImageUrl)
                    {
                        ModelState.AddModelError("Pie.ImageUrl", $"Current value: {databaseValues.ImageUrl}");
                    }
                    if (databaseValues.IsPieOfTheWeek != entityValues.IsPieOfTheWeek)
                    {
                        ModelState.AddModelError("Pie.IsPieOfTheWeek", $"Current value: {databaseValues.IsPieOfTheWeek}");
                    }
                    if (databaseValues.IsInStock != entityValues.IsInStock)
                    {
                        ModelState.AddModelError("Pie.InStock", $"Current value: {databaseValues.IsInStock}");
                    }
                    if (databaseValues.CategoryId != entityValues.CategoryId)
                    {
                        ModelState.AddModelError("Pie.CategoryId", $"Current value: {databaseValues.CategoryId}");
                    }

                    ModelState.AddModelError(string.Empty, "The pie was modified already by another user. The database values are now shown. Hit Save again to store these values.");

                    pieToUpdate!.RowVersion = databaseValues.RowVersion;

                    ModelState.Remove("Pie.RowVersion");
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
