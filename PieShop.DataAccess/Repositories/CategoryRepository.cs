﻿using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using CategoryEntity = PieShop.DataAccess.Data.Entitites.Category.Category;
using CategoryModel = PieShop.Models.Category.Category;
using PieModel = PieShop.Models.Pie.Pie;

namespace PieShop.DataAccess.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly PieShopContext _pieShopContext;
        private readonly IOutputCacheStore _outputCacheStore;
        private readonly ILogger<CategoryRepository> _logger;

        private readonly string[] _tagsArray = new string[] { allCategoriesCacheKey };

        const string allCategoriesCacheKey = "allCategories";

        public CategoryRepository(PieShopContext pieShopContext, IOutputCacheStore outputCacheStore, ILogger<CategoryRepository> logger)
        {
            _pieShopContext = pieShopContext;
            _outputCacheStore = outputCacheStore;
            _logger = logger;
        }

        public async Task<IEnumerable<CategoryModel>> GetAllCategoriesAsync()
        {
            var cachedCategories = await _outputCacheStore.GetAsync(allCategoriesCacheKey, CancellationToken.None);

            if (cachedCategories != null)
            {
                _logger.LogInformation("Using cached categories");
                var categories = JsonSerializer.Deserialize<List<CategoryModel>>(cachedCategories);
                return categories ?? new List<CategoryModel>();
            }

            // Test OutputCache
            await Task.Delay(TimeSpan.FromSeconds(3));

            _logger.LogInformation("Fetching categories from the database");

            var allCategories = await _pieShopContext.Category
                    .AsNoTracking()
                    .Select(category => new CategoryModel
                    {
                        CategoryId = category.CategoryId,
                        Name = category.Name,
                        Description = category.Description,
                    })
                    .OrderBy(category => category.Name)
                    .ToListAsync();

            var serializedCategories = JsonSerializer.Serialize(allCategories);

            var encodedCategories = Encoding.UTF8.GetBytes(serializedCategories);

            await _outputCacheStore.SetAsync(allCategoriesCacheKey, encodedCategories, _tagsArray, TimeSpan.FromSeconds(60), CancellationToken.None);

            _logger.LogInformation("Caching {Count} categories", allCategories.Count);

            return allCategories;
        }

        public async Task<int> AddCategoryAsync(CategoryModel category)
        {
            var categoryEntity = new CategoryEntity
            {
                Name = category.Name,
                Description = category.Description
            };

            bool categoryWithSameNameExist = await _pieShopContext.Category.AnyAsync(c => c.Name == category.Name);

            if (categoryWithSameNameExist)
            {
                throw new Exception("A category with the same name already exists");
            }

            await _pieShopContext.AddAsync(categoryEntity);

            int result = await _pieShopContext.SaveChangesAsync();

            _logger.LogInformation("Cache evicted for all categories after adding a new category.");

            await _outputCacheStore.EvictByTagAsync(allCategoriesCacheKey, CancellationToken.None);

            return result;
        }

        public async Task<CategoryModel?> GetCategoryByCategoryIdAsync(Guid categoryId)
        {
            //TODO: verify if pies are null

            return await _pieShopContext.Category
                .AsNoTracking()
                .Include(c => c.Pies)
                .Where(category => category.CategoryId == categoryId)
                .Select(category => new CategoryModel
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name,
                    Description = category.Description,
                    Pies = category.Pies.Select(p => new PieModel
                    {
                        PieId = p.PieId,
                        Name = p.Name,
                        ShortDescription = p.ShortDescription,
                        LongDescription = p.LongDescription,
                        AllergyInformation = p.AllergyInformation,
                        Price = p.Price,
                        ImageUrl = p.ImageUrl,
                        ImageThumbnailUrl = p.ImageThumbnailUrl,
                        IsPieOfTheWeek = p.IsPieOfTheWeek,
                        IsInStock = p.IsInStock
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<int> UpdateCategoryAsync(CategoryModel category)
        {
            bool categoryWithSameNameExist = await _pieShopContext.Category.AnyAsync(c => c.Name == category.Name && c.CategoryId != category.CategoryId);

            if (categoryWithSameNameExist)
            {
                throw new Exception("A category with the same name already exists");
            }

            var categoryToUpdate = await _pieShopContext.Category.FirstOrDefaultAsync(c => c.CategoryId == category.CategoryId);

            if (categoryToUpdate != null)
            {
                categoryToUpdate.Name = category.Name;
                categoryToUpdate.Description = category.Description;

                _pieShopContext.Category.Update(categoryToUpdate);

                var result = await _pieShopContext.SaveChangesAsync();

                await _outputCacheStore.EvictByTagAsync(allCategoriesCacheKey, CancellationToken.None);

                return result;
            }
            else
            {
                throw new ArgumentException($"The category to update can't be found.");
            }
        }

        public async Task<int> DeleteCategoryAsync(Guid categoryId)
        {
            var categoryToDelete = await _pieShopContext.Category.FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (categoryToDelete != null)
            {
                _pieShopContext.Category.Remove(categoryToDelete);

                var result = await _pieShopContext.SaveChangesAsync();

                await _outputCacheStore.EvictByTagAsync(allCategoriesCacheKey, CancellationToken.None);

                return result;
            }
            else
            {
                throw new ArgumentException($"The category to delete can't be found.");
            }
        }
    }
}
