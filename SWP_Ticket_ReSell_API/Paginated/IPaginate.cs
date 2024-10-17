namespace SWP_Ticket_ReSell_API.Paginated
{
    public interface IPaginate<TResult>
    {
        int Size { get; }
        int Page { get; }
        int Total { get; }
        int TotalPages { get; }
        IList<TResult> Items { get; }
    }
}
