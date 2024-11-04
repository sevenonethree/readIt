namespace ReadIt.Services;

// no longer used... Decided I only really need the paginated source
// since if next page is null or empty, it will just get the first page.
public interface IExternalPostSource<T>
{
    Task<T> GetPostsAsync(ApiMetadata metadata);
}

public interface IPaginatedPostSource<T>
{
    Task<T> GetNextPage(PaginatedApiMetaData metadata);
}

public record ApiMetadata
{
    public required string Category { get; init; }
}

public record PaginatedApiMetaData : ApiMetadata
{
    public string PreviousPageId { get; init; } = string.Empty;
    public string NextPageId { get; init; } = string.Empty;

}
