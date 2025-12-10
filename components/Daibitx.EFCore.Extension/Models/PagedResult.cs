namespace Daibitx.EFCore.Extension.Models
{
    public class PagedResult<T>
    {
        public int TotalCount { get; }
        public int PageIndex { get; }
        public int PageSize { get; }
        public IReadOnlyList<T> Items { get; }

        public PagedResult(List<T> items, int count, int pageIndex, int pageSize)
        {
            Items = items;
            TotalCount = count;
            PageIndex = pageIndex;
            PageSize = pageSize;
        }
    }
}
