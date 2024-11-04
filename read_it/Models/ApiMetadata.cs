namespace ReadIt.Models;

public record ApiMetadata
{
    public required string Category { get; init; }
}

public record PaginatedApiMetaData : ApiMetadata
{
    public string PreviousPageId { get; init; } = string.Empty;
    public string NextPageId { get; init; } = string.Empty;

}
