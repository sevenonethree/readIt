using System.Text.Json.Serialization;

namespace ReadIt.Models.Reddit;

public record RedditAccessTokenResponse
{
    [JsonPropertyName("access_token")]
    public required string AccessToken {get; init;}
    [JsonPropertyName("token_type")]
    public required string TokenType {get; init;}
    [JsonPropertyName("expires_in")]
    public int ExpiresIn {get; init;}
    [JsonPropertyName("scope")]
    public required string Scope {get; init;}

    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
