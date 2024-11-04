namespace ReadIt.Models;

public record PaginatedApiResponse<T>
{
    public int PageNumber { get; set; }
    public int TotalItemCount { get; set; }
    public IEnumerable<T>? Data { get; set; }
}
