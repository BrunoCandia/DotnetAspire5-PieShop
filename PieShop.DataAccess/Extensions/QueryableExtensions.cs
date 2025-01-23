using System.Linq.Expressions;

namespace PieShop.DataAccess.Extensions
{
    public static class QueryableExtensions
    {
        public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(this IQueryable<TSource> queryable, Expression<Func<TSource, TKey>> keySelector, bool orderByDescending)
        {
            return orderByDescending ? queryable.OrderByDescending(keySelector) : queryable.OrderBy(keySelector);
        }
    }
}