using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ReadIt.Models;
using ReadIt.Models.Reddit;
using Polly;
using Polly.RateLimit;
using Serilog;

namespace ReadIt.Services;

public class RedditApi : IPaginatedPostSource<RedditResponse?>
{
    private readonly HttpClient _client;
    private RedditAccessTokenResponse? _authorization;
    private RedditOptions _redditOptions;

    public RedditApi(IHttpClientFactory factory, IOptions<RedditOptions> options)
    {
        // _factory = factory;
        _client = factory.CreateClient("Reddit");
        _redditOptions = options.Value;
    }

    private async Task<RedditAccessTokenResponse> Authorize()
    {
        var credentials = $"{_redditOptions.AppId}:{_redditOptions.ClientSecret}";

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{_redditOptions.BaseUrl}/api/v1/access_token"),
            Headers =
            {
                { "User-Agent", $"Mozilla/5.0 ({_redditOptions.UserAgent})" },
                { "Authorization", $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials))}" },
            },
            Content = new MultipartFormDataContent
            {
                new StringContent("client_credentials")
                {
                    Headers =
                    {
                        ContentDisposition = new ContentDispositionHeaderValue("form-data")
                        {
                            Name = "grant_type",
                        }
                    }
                },
            },
        };

        using var response = SendHttpMessage(request);

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        var token = JsonSerializer.Deserialize<RedditAccessTokenResponse>(body) ?? new()
        {
            AccessToken = string.Empty,
            TokenType = string.Empty,
            Scope = string.Empty,
        };
        ;
        token.CreatedAt = DateTime.UtcNow;
        token.ExpiresAt = DateTime.UtcNow.AddSeconds(token.ExpiresIn);

        return token;
    }

    public async Task<RedditResponse?> GetRedditPostsAsync(string subReddit, string after = "")
    {
        try
        {
            if (string.IsNullOrEmpty(subReddit)) throw new ArgumentException("Argument subReddit is required");

            if (_authorization == null || DateTime.UtcNow > _authorization?.ExpiresAt)
                _authorization = await Authorize();

            var request = BuildBasePostsRequestMessage();
            request.RequestUri = new Uri($"{_redditOptions.BaseUrl}/r/{subReddit}/top?t=all&limit=100{(string.IsNullOrEmpty(after) ? "" : $"&after={after}")}");

            using var response = SendHttpMessage(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<RedditResponse>();
        }
        catch (Exception ex)
        {
            // TODO: There should be more robust error handling here. 
            // Adding it at this level as there is probably something specific you would want to do for each 
            // post source.
            Log.Error(ex, "An exception occurred when attempting to process posts from Reddit in {methodName}", new { methodName = nameof(GetRedditPostsAsync) });
        }

        return new RedditResponse()
        {
            Kind = "Error",
            ListingInfo = new()
        };
    }


    // TODO: Find a way to generalize this so it isn't specific to Reddit... 
    // Will require more time and thought, but don't want to keep you guys waiting!
    public async Task<RedditResponse?> GetNextPage(PaginatedApiMetaData metaData)
    {
        return await GetRedditPostsAsync(metaData.Category, after: metaData.NextPageId);
    }

    private HttpRequestMessage BuildBasePostsRequestMessage()
    {
        return new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            Headers =
            {
                { "User-Agent", $"Mozilla/5.0 ({_redditOptions.UserAgent})" },
                { "Authorization", $"Bearer {_authorization?.AccessToken}" },
            },
            Content = new MultipartFormDataContent
            {
                new StringContent("client_credentials")
                {
                    Headers =
                    {
                        ContentDisposition = new ContentDispositionHeaderValue("form-data")
                        {
                            Name = "grant_type",
                        }
                    }
                },
            },
        };
    }

    private HttpResponseMessage SendHttpMessage(HttpRequestMessage msg)
    {
        var rateLimitPolicy = Policy.RateLimit(100, TimeSpan.FromMinutes(1));

        try
        {
            return rateLimitPolicy.Execute(async () => await _client.SendAsync(msg)).Result;
        }
        catch (RateLimitRejectedException rex)
        {
            // Logging debug as this shouldn't affect the user
            Log.Debug(rex, "Rate Limit exceeded");
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected exception encountered in {methodName}!", nameof(SendHttpMessage));
            throw;
        }
    }
}
