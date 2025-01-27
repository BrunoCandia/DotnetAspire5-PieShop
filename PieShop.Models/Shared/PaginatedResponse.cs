namespace PieShop.Models.Shared
{
    public class PaginatedResponse<T>
    {
        public T Data { get; set; } = default!;

        public int TotalRecords { get; set; }

        public int TotalPages { get; set; }

        public int PageIndex { get; set; }

        public bool HasPreviousPage => PageIndex > 1;

        public bool HasNextPage => PageIndex < TotalPages;
    }
}