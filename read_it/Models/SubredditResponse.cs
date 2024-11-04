using System.Text.Json.Serialization;
using ReadIt.Models;

public record RedditResponse : IPost
{
    public required string Kind { get; init; }

    [JsonPropertyName("data")]
    public required RedditListingWrapper ListingInfo { get; init; }
}


public record RedditListingWrapper
{
    public string? Kind { get; init; }

    [JsonPropertyName("after")]
    public string? NextPage { get; init; }

    [JsonPropertyName("before")]
    public string? PreviousPage { get; init; }

    [JsonPropertyName("children")]
    public IEnumerable<RedditPost>? Posts { get; init; }

}

public record RedditPost
{
    public static readonly string TypeIdentifier = "t3";
    public required string Kind { get; init; }

    [JsonPropertyName("data")]
    public required PostDetails Details { get; init; }
}

public record PostDetails : IRepositoryItem
{
    // [JsonPropertyName("id")]
    public required string Id { get; set; }

    // [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("author_fullname")]
    public string? AuthorFullName { get; init; }

    [JsonPropertyName("author")]
    public required string AuthorUserName { get; init; }

    public required string Title { get; init; }

    [JsonPropertyName("ups")]
    public long Upvotes { get; init; }

    [JsonPropertyName("upvote_ratio")]
    public decimal UpvoteRatio { get; init; }

    public required string SubReddit { get; init; }
}

public record RedditUser : IRepositoryItem
{
    [JsonPropertyName("author")]
    public required string UserName { get; init; }

    public IEnumerable<RedditPost>? Posts { get; init; }
    public required string Id { get; set; }
}

public record ActiveRedditUser : IRepositoryItem
{
    public required string UserName { get; set; }
    public int PostCount { get; set; }
    public string Id { get; set; }
}
