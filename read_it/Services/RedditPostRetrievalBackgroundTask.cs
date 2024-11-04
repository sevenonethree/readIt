using Microsoft.Extensions.Options;
using ReadIt.Models;
using ReadIt.Models.Reddit;
using ReadIt.Repositories;
using Serilog;

namespace ReadIt.Services;

public interface IBackgroundTaskWorker
{
    void Execute();
}
public class RedditPostRetrievalBackgroundTask : IBackgroundTaskWorker
{
    public readonly RedditOptions _redditSettings;
    public readonly IPaginatedPostSource<RedditResponse> _source;
    public readonly IRepository<PostDetails> _postRepository;
    private Dictionary<string, PaginatedApiMetaData> nextPageDictionary;

    public RedditPostRetrievalBackgroundTask
    (
        IOptions<RedditOptions> redditSettings,
        IPaginatedPostSource<RedditResponse> source,
        IRepository<PostDetails> repository
    )
    {
        _redditSettings = redditSettings.Value;
        _source = source;
        _postRepository = repository;

        nextPageDictionary = new();
    }

    // TODO: Add a way to cancel this request. As it stands, it cannot be tested in a meaningful way.
    public void Execute()
    {
        while (true)
        {
            try
            {
                _redditSettings.SubReddits
                    .ToList()
                    .ForEach(async subReddit =>
                    {
                        if (!nextPageDictionary.ContainsKey(subReddit))
                        {
                            nextPageDictionary[subReddit] = new() { Category = subReddit };
                        }
                        // var nextPageInformation = 
                        RedditResponse response = await _source.GetNextPage(nextPageDictionary[subReddit]);

                        // TODO: Highly inefficient. Need to add a way to save a list of post details
                        response?.ListingInfo?.Posts
                            ?.ToList()
                            .ForEach(post =>
                            {
                                _postRepository.Save(post.Details);
                            });

                        nextPageDictionary[subReddit] = new PaginatedApiMetaData()
                        {
                            Category = subReddit,
                            NextPageId = response?.ListingInfo.NextPage ?? string.Empty,
                            PreviousPageId = response?.ListingInfo.PreviousPage ?? string.Empty
                        };
                    });
            }
            catch (Exception ex)
            {
                // TODO: Handle any errors that make sense here, or possibly stop the background retrieval to 
                // save on rate limit/money. 
                // There should DEFINITELY be some means of notifying people if the background process is stopped.
                Log.Error(
                    ex,
                    "An unexpected error occurred in RedditPostRetrievalBackgroundTask.{methodName}. The message was: {Message}",
                    new { methodName = nameof(Execute), ex.Message }
                );
            }
        }
    }

    // Due to time constraints, I am not implementing this.
    // I tend to offer an async option for almost everything
    // not sure if it fully makes sense here.
    public Task ExecuteAsync()
    {
        Execute();
        return Task.FromResult(true);
    }
}
