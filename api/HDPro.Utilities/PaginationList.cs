using System.Diagnostics.CodeAnalysis;

namespace HDPro.Utilities
{
    public class PaginationList<TElement>
    {
        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages { get; private set; }
        /// <summary>
        /// 总数量
        /// </summary>
        public int TotalCount { get; private set; }
        /// <summary>
        /// 当前页
        /// </summary>
        public int CurrentPage { get; set; }
        /// <summary>
        /// 是否有前一页
        /// </summary>
        public bool HasPrevious => CurrentPage > 1;
        /// <summary>
        /// 是否有下一页
        /// </summary>
        public bool HasNext => CurrentPage < TotalPages;
        /// <summary>
        /// 每页大小
        /// </summary>
        public int PageSize { get; set; }
        public IList<TElement> Data { get; set; }

        public PaginationList(IList<TElement> items,int totalcount, int pageSize, int pageIndex)
        {
            Data = items;
            CurrentPage = pageIndex;
            PageSize = pageSize;
            TotalCount = totalcount;
            TotalPages = (totalcount - 1) / pageSize + 1;

        }
        public TElement this[int index] => Data[index];
    
    }
    public static class PaginationListExtensions
    {
        public static PaginationList<T> ToPaginationList<T>([NotNull] this IEnumerable<T> data, int totalCount, int pageSize,
            int pageIndex = 1)
        {
            return new(data.ToList(), totalCount, pageSize, pageIndex); ;
        }

        public static PaginationList<T> ToPaginationList<T>([NotNull] this IEnumerable<T> data, int totalCount, IPagination pagination)
        {
            return data.ToPaginationList(totalCount, pagination.PageSize, pagination.PageIndex);
        }
    }
}
