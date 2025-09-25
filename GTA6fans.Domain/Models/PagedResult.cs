namespace GTA6fans.Domain.Models;

public class PagedResult<T>
{
    public List<T> Items { get; set; }
    public long TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
