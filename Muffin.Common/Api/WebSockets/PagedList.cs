using System.Collections.Generic;

namespace Muffin.Common.Api.WebSockets
{
    public class PagedList<T>
    {
        public List<T> Data { get; set; }

        public int DataCount { get; set; }
        public int TotalCount { get; set; }
        public int PageStart { get; set; }
        public int PageSize { get; set; }
        public int PageCount { get; set; }
        public string Order { get; set; }
        public string SearchString { get; set; }
        public bool OrderAscending { get; set; }

#warning TODO
        //public PagedFilterRequest<T> NextPageRequest()
        //{
        //    return new PagedFilterRequest<T>()
        //    {
        //        Order = Order,
        //        OrderAscending = OrderAscending,
        //        PageSize = PageSize,
        //        SearchString = SearchString,
        //        PageStart = Math.Min(PageStart + 1, PageCount)
        //    };
        //}

        //public PagedFilterRequest<T> PreviousPageRequest()
        //{
        //    return new PagedFilterRequest<T>()
        //    {
        //        Order = Order,
        //        OrderAscending = OrderAscending,
        //        PageSize = PageSize,
        //        SearchString = SearchString,
        //        PageStart = Math.Max(PageStart - 1, 0)
        //    };
        //}
    }
}
