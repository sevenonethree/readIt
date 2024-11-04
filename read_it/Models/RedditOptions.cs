using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ReadIt.Models.Reddit;

public class RedditOptions
{
    public static readonly string Name = "RedditSettings";

    public required string AppId { get; set; }
    public required string BaseUrl { get; set; }
    public required string ClientSecret { get; set; }
    public required IEnumerable<string> SubReddits { get; set; }
    public required string UserAgent { get; set; }

}
