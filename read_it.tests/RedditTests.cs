using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using ReadIt.Models.Reddit;
using ReadIt.Services;
namespace ReadIt.tests;

public class RedditTests
{
    [Fact]
    public async Task CanDeserializeSubRedditResponse()
    {
        // This mock will allow us to validate the response information in mockSubreddit.json
        // without having to make real calls to the api. Given more time, I would create a
        // pipeline that allows me to update the data to ensure that the response doesn't 
        // change to something we can't deserialize over time. 
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Is("Reddit")).Returns(new HttpClient(new MockMessageHandler()));

        RedditOptions redditOptions = new()
        {
            AppId = "Fake",
            ClientSecret = "not-real",
            SubReddits = new List<string>() { "testing" },
            UserAgent = "Mozilla/5.0 (testing api)",
            BaseUrl = "https://localhost.com"
        };

        var sut = new RedditApi(factory, Options.Create(redditOptions));

        var actual = await sut.GetRedditPostsAsync("localTest", "");

        actual?.ListingInfo?.Posts?.Any().Should().BeTrue();
        actual?.ListingInfo?.Posts?.All(post => post.Details != null && !string.IsNullOrEmpty(post.Details.Id)).Should().BeTrue();
    }
}

public class MockMessageHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Currently there are only two calls being made to the reddit api. 
        // As this grows, it would make sense to handle these forks better...
        // A potential idea would be to utilize the strategy pattern to centralize
        // the logic for mocks... so you only have to update one location to 
        // mock different responses
        if (request?.RequestUri?.AbsoluteUri.Contains("access_token") ?? false)
        {
            return Task.FromResult(new HttpResponseMessage()
            {
                Content = JsonContent.Create(new RedditAccessTokenResponse()
                {
                    ExpiresIn = 18000,
                    Scope = "read",
                    AccessToken = "12345",
                    TokenType = string.Empty
                })
            });
        }
        else
        {
            var responseText = File.ReadAllText("mockSubredditResponse.json");

            return Task.FromResult(new HttpResponseMessage()
            {
                Content = new StringContent(responseText),
            });
        }
    }
}
