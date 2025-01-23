using Microsoft.EntityFrameworkCore;
using PieShop.DataAccess.Extensions;
using PieShop.Models.Shared;
using CategoryModel = PieShop.Models.Category.Category;
using IngredientModel = PieShop.Models.Pie.Ingredient;
using PieEntity = PieShop.DataAccess.Data.Entitites.Pie.Pie;
using PieModel = PieShop.Models.Pie.Pie;

namespace PieShop.DataAccess.Repositories
{
    public class PieRepository : IPieRepository
    {
        private readonly PieShopContext _pieShopContext;

        public PieRepository(PieShopContext pieShopContext)
        {
            _pieShopContext = pieShopContext;
        }

        public async Task<PieModel?> GetPieByPieIdAsync(Guid pieId)
        {
            return await _pieShopContext.Pie
                .AsNoTracking()
                .Include(c => c.Category)
                .Where(pie => pie.PieId == pieId)
                .Select(pie => new PieModel
                {
                    PieId = pie.PieId,
                    Name = pie.Name,
                    ShortDescription = pie.ShortDescription,
                    LongDescription = pie.LongDescription,
                    AllergyInformation = pie.AllergyInformation,
                    Price = pie.Price,
                    ImageUrl = pie.ImageUrl,
                    ImageThumbnailUrl = pie.ImageThumbnailUrl,
                    IsPieOfTheWeek = pie.IsPieOfTheWeek,
                    IsInStock = pie.IsInStock,
                    CategoryId = pie.CategoryId,
                    Category = new CategoryModel
                    {
                        CategoryId = pie.Category.CategoryId,
                        Name = pie.Category.Name
                    }
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PieModel>> GetAllPiesAsync()
        {
            return await _pieShopContext.Pie
                    .AsNoTracking()
                    .Include(c => c.Category)
                    .Select(pie => new PieModel
                    {
                        PieId = pie.PieId,
                        Name = pie.Name,
                        ShortDescription = pie.ShortDescription,
                        LongDescription = pie.LongDescription,
                        AllergyInformation = pie.AllergyInformation,
                        Price = pie.Price,
                        ImageUrl = pie.ImageUrl,
                        ImageThumbnailUrl = pie.ImageThumbnailUrl,
                        IsPieOfTheWeek = pie.IsPieOfTheWeek,
                        IsInStock = pie.IsInStock,
                        CategoryId = pie.CategoryId,
                        Category = new CategoryModel
                        {
                            CategoryId = pie.Category.CategoryId,
                            Name = pie.Category.Name,
                            Description = pie.Category.Description
                        }
                    })
                    .ToListAsync();
        }

        public async Task<IEnumerable<PieModel>> GetPiesOfTheWeekAsync()
        {
            return await _pieShopContext.Pie
                    .AsNoTracking()
                    .Include(c => c.Category)
                    .Where(pie => pie.IsPieOfTheWeek)
                    .Select(pie => new PieModel
                    {
                        PieId = pie.PieId,
                        Name = pie.Name,
                        ShortDescription = pie.ShortDescription,
                        LongDescription = pie.LongDescription,
                        AllergyInformation = pie.AllergyInformation,
                        Price = pie.Price,
                        ImageUrl = pie.ImageUrl,
                        ImageThumbnailUrl = pie.ImageThumbnailUrl,
                        IsPieOfTheWeek = pie.IsPieOfTheWeek,
                        IsInStock = pie.IsInStock,
                        CategoryId = pie.CategoryId,
                        Category = new CategoryModel
                        {
                            CategoryId = pie.Category.CategoryId,
                            Name = pie.Category.Name,
                            Description = pie.Category.Description
                        }
                    })
                    .ToListAsync();
        }

        public async Task<IEnumerable<PieModel>> SearchPiesAsync(string searchQuery, string? categoryId)
        {
            var query = GetBaseQueryForSearch();
            query = FilterWhereCluaseForSearch(query, searchQuery, categoryId);

            var result = await query.ToListAsync();

            var mappedResult = result.Select(x => new PieModel
            {
                PieId = x.PieId,
                Name = x.Name,
                ShortDescription = x.ShortDescription,
                LongDescription = x.LongDescription,
                AllergyInformation = x.AllergyInformation,
                Price = x.Price,
                ImageUrl = x.ImageUrl,
                ImageThumbnailUrl = x.ImageThumbnailUrl,
                IsPieOfTheWeek = x.IsPieOfTheWeek,
                IsInStock = x.IsInStock,
                CategoryId = x.CategoryId
            });

            return mappedResult;
        }

        private IQueryable<PieEntity> GetBaseQueryForSearch()
        {
            return _pieShopContext.Pie
                .AsNoTracking()
                .AsQueryable();
        }

        private static IQueryable<PieEntity> FilterWhereCluaseForSearch(IQueryable<PieEntity> query, string searchQuery, string? categoryId)
        {
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(s => s.Name.Contains(searchQuery) ||
                                             (s.ShortDescription != null && s.ShortDescription.Contains(searchQuery)) ||
                                             (s.LongDescription != null && s.LongDescription.Contains(searchQuery)));
            }

            if (categoryId != null)
            {
                var categoryIdTransformed = new Guid(categoryId);
                query = query.Where(s => s.CategoryId == categoryIdTransformed);
            }

            return query;
        }

        ////public async Task<IEnumerable<Pie>> SearchPiesAsync(string searchQuery)
        ////{
        ////    return await _pieShopContext.Pie
        ////        .AsNoTracking()
        ////        .Where(p => p.Name.Contains(searchQuery))
        ////        .Select(pie => new Pie
        ////        {
        ////            PieId = pie.PieId,
        ////            Name = pie.Name,
        ////            ShortDescription = pie.ShortDescription,
        ////            LongDescription = pie.LongDescription,
        ////            AllergyInformation = pie.AllergyInformation,
        ////            Price = pie.Price,
        ////            ImageUrl = pie.ImageUrl,
        ////            ImageThumbnailUrl = pie.ImageThumbnailUrl,
        ////            IsPieOfTheWeek = pie.IsPieOfTheWeek,
        ////            IsInStock = pie.IsInStock,
        ////            CategoryId = pie.CategoryId,
        ////            Category = new Category
        ////            {
        ////                CategoryId = pie.Category.CategoryId,
        ////                Name = pie.Category.Name,
        ////                Description = pie.Category.Description
        ////            }
        ////        })
        ////        .ToListAsync();
        ////}

        public async Task<PaginatedResponse<IEnumerable<PieModel>>> GetPiesPaginatedAsync(string orderBy, bool orderByDescending, int pageNumber, int pageSize)
        {
            var query = GetBaseQueryForPagination();
            query = OrderByClauseForPagination(query, orderBy, orderByDescending);

            var totalIems = await query.CountAsync();

            if (totalIems == 0)
            {
                return new PaginatedResponse<IEnumerable<PieModel>>(); //TODO: review!!!
            }

            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            var result = await query.ToListAsync();

            var mappedResult = result.Select(x => new PieModel
            {
                PieId = x.PieId,
                Name = x.Name,
                ShortDescription = x.ShortDescription,
                LongDescription = x.LongDescription,
                AllergyInformation = x.AllergyInformation,
                Price = x.Price,
                ImageUrl = x.ImageUrl,
                ImageThumbnailUrl = x.ImageThumbnailUrl,
                IsPieOfTheWeek = x.IsPieOfTheWeek,
                IsInStock = x.IsInStock,
                CategoryId = x.CategoryId,
                Category = new CategoryModel
                {
                    CategoryId = x.Category.CategoryId,
                    Name = x.Category.Name,
                    Description = x.Category.Description
                }
            });

            return new PaginatedResponse<IEnumerable<PieModel>>
            {
                Data = mappedResult,
                TotalRecords = totalIems,
                TotalPages = (int)Math.Ceiling(totalIems / (double)pageSize),
                PageIndex = pageNumber
            };
        }

        private IQueryable<PieEntity> GetBaseQueryForPagination()
        {
            return _pieShopContext.Pie
                .AsNoTracking()
                .Include(x => x.Category)
                .AsQueryable();
        }

        private static IQueryable<PieEntity> OrderByClauseForPagination(IQueryable<PieEntity> query, string orderBy, bool orderByDescending)
        {
            query = orderBy switch
            {
                "name" => query.OrderBy(x => x.Name, orderByDescending),
                "price" => query.OrderBy(x => x.Price, orderByDescending),
                _ => query.OrderBy(x => x.PieId, orderByDescending)
            };

            return query;
        }

        public async Task<int> AddPieAsync(PieModel pieModel)
        {
            var pieEntity = new PieEntity
            {
                Name = pieModel.Name,
                ShortDescription = pieModel.ShortDescription,
                LongDescription = pieModel.LongDescription,
                AllergyInformation = pieModel.AllergyInformation,
                Price = pieModel.Price,
                ImageUrl = pieModel.ImageUrl,
                ImageThumbnailUrl = pieModel.ImageThumbnailUrl,
                IsPieOfTheWeek = pieModel.IsPieOfTheWeek,
                IsInStock = pieModel.IsInStock,
                CategoryId = pieModel.CategoryId,
            };

            //TODO: verificar name es unico

            await _pieShopContext.AddAsync(pieEntity);

            return await _pieShopContext.SaveChangesAsync();
        }

        public async Task<int> UpdatePieAsync(PieModel pie)
        {
            var pieToUpdate = await _pieShopContext.Pie.FirstOrDefaultAsync(c => c.PieId == pie.PieId);

            if (pieToUpdate != null)
            {
                ////_pieShopContext.Entry(pieToUpdate).Property("RowVersion").OriginalValue = pie.RowVersion;

                pieToUpdate.CategoryId = pie.CategoryId;
                pieToUpdate.ShortDescription = pie.ShortDescription;
                pieToUpdate.LongDescription = pie.LongDescription;
                pieToUpdate.Price = pie.Price;
                pieToUpdate.AllergyInformation = pie.AllergyInformation;
                pieToUpdate.ImageThumbnailUrl = pie.ImageThumbnailUrl;
                pieToUpdate.ImageUrl = pie.ImageUrl;
                pieToUpdate.IsInStock = pie.IsInStock;
                pieToUpdate.IsPieOfTheWeek = pie.IsPieOfTheWeek;
                pieToUpdate.Name = pie.Name;

                _pieShopContext.Pie.Update(pieToUpdate);

                return await _pieShopContext.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException($"The pie to update can't be found.");
            }
        }

        public async Task<int> DeletePieAsync(Guid pieId)
        {
            var pieToDelete = await _pieShopContext.Pie.FirstOrDefaultAsync(p => p.PieId == pieId);

            if (pieToDelete != null)
            {
                _pieShopContext.Pie.Remove(pieToDelete);

                return await _pieShopContext.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException($"The pie to delete can't be found.");
            }
        }

        public async Task<PieModel?> GetFullPieByPieIdAsync(Guid pieId)
        {
            //TODO: add ingredients in the query

            return await _pieShopContext.Pie
                .AsNoTracking()
                .Include(c => c.Category)
                .Include(i => i.Ingredients)
                .Where(pie => pie.PieId == pieId)
                .Select(pie => new PieModel
                {
                    PieId = pie.PieId,
                    Name = pie.Name,
                    ShortDescription = pie.ShortDescription,
                    LongDescription = pie.LongDescription,
                    AllergyInformation = pie.AllergyInformation,
                    Price = pie.Price,
                    ImageUrl = pie.ImageUrl,
                    ImageThumbnailUrl = pie.ImageThumbnailUrl,
                    IsPieOfTheWeek = pie.IsPieOfTheWeek,
                    IsInStock = pie.IsInStock,
                    CategoryId = pie.CategoryId,
                    Category = new CategoryModel
                    {
                        CategoryId = pie.Category.CategoryId,
                        Name = pie.Category.Name
                    },
                    Ingredients = pie.Ingredients.Select(i => new IngredientModel
                    {
                        IngredientId = i.IngredientId,
                        Name = i.Name,
                        Amount = i.Amount,
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }
    }
}
