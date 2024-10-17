using Microsoft.EntityFrameworkCore;

namespace SWP_Ticket_ReSell_API.Paginated
{
    public class PaginatedList<TResult> : IPaginate<TResult>
    {
        public int Size { get; private set; }
        public int Page { get; private set; }
        public int Total { get; private set; }
        public int TotalPages { get; private set; }
        public IList<TResult> Items { get; private set; }

        public PaginatedList(IList<TResult> items, int total, int page, int size)
        {
            Total = total;
            Page = page;
            Size = size;
            TotalPages = (int)Math.Ceiling(total / (double)size);
            Items = items;
        }

        public static PaginatedList<TResult> Create(IList<TResult> source, int page, int size)
        {
            var total = source.Count;
            var items = source.Skip((page - 1) * size).Take(size).ToList();
            return new PaginatedList<TResult>(items, total, page, size);
        }
    }
}
