namespace AppTech.Core.Entities.Commons
{
    public class PaginatedQueryable<T>
    {
        public IQueryable<T> Items { get; }
        public int PageIndex { get; }
        public int TotalPages { get; }

        public PaginatedQueryable(IQueryable<T> items, int pageIndex, int totalPages)
        {
            Items = items;
            PageIndex = pageIndex;
            TotalPages = totalPages;
        }
    }
}
