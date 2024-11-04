using Microsoft.Extensions.Options;
using NSubstitute.ExceptionExtensions;
using ReadIt.Models;
using ReadIt.Models.Reddit;
using ReadIt.Repositories;
using ReadIt.Services;

namespace ReadIt.Tests;

public class BackgroundTaskTests
{
    [Fact]
    public void CanCancelBackgroundTest()
    {
        RedditOptions options = new()
        {
            SubReddits = new List<string>() { "testing" },
            AppId = "12345",
            ClientSecret = "12345",
            BaseUrl = "http://localhost.com",
            UserAgent = "Mozilla/5.0 (test agent)"
        };
        var redditSource = Substitute.For<IPaginatedPostSource<RedditResponse>>();
        var repo = Substitute.For<IRepository<PostDetails>>();

        var tokenSource = new CancellationTokenSource();
        var token = tokenSource.Token;

        redditSource
            .GetNextPage(Arg.Any<PaginatedApiMetaData>())
            .Returns(Task.FromResult(new RedditResponse
            {
                Kind = "",
                ListingInfo = new(),
            }));

        var sut = new RedditPostRetrievalBackgroundTask(Options.Create(options), redditSource, repo);
        // Wait 3 seconds and attempt to cancel the token
        var cancelTask = Task.Run(() =>
        {
            Thread.Sleep(3000);
            tokenSource.Cancel();
        });

        sut.ExecutionTimeOf(s => s.Execute(token)).Should().BeLessThan(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void CanHandleExceptionGracefully()
    {
        RedditOptions options = new()
        {
            SubReddits = new List<string>() { "testing" },
            AppId = "12345",
            ClientSecret = "12345",
            BaseUrl = "http://localhost.com",
            UserAgent = "Mozilla/5.0 (test agent)"
        };
        var redditSource = Substitute.For<IPaginatedPostSource<RedditResponse>>();
        var repo = Substitute.For<IRepository<PostDetails>>();

        var tokenSource = new CancellationTokenSource();
        var token = tokenSource.Token;

        redditSource
            .GetNextPage(Arg.Any<PaginatedApiMetaData>())
            // .When(source => source.GetNextPage(Arg.Any<PaginatedApiMetaData>()))
            .Throws(x => throw new Exception("I am but a humble test exception"));

        var sut = new RedditPostRetrievalBackgroundTask(Options.Create(options), redditSource, repo);

        // Wait 3 seconds and attempt to cancel the token
        var cancelTask = Task.Run(() =>
        {
            Thread.Sleep(3000);
            tokenSource.Cancel();
        });

        var act = () => sut.Execute(token);
        act.Should().NotThrow();
    }
}
