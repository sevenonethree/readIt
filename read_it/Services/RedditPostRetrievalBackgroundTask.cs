using Microsoft.Extensions.Options;
using ReadIt.Models.Reddit;
using ReadIt.Repositories;

namespace ReadIt.Services;

public interface IBackgroundTaskWorker
{
    void Execute();
    Task ExecuteAsync();
}
public class RedditPostRetrievalBackgroundTask: IBackgroundTaskWorker
{
    public readonly RedditOptions _redditSettings;
    public readonly IPaginatedPostSource<RedditResponse> _source;
    public readonly IRepository<PostDetails> _repository;
    private Dictionary<string, PaginatedApiMetaData> nextPageDictionary = new();

    public RedditPostRetrievalBackgroundTask(IOptions<RedditOptions> redditSettings, IPaginatedPostSource<RedditResponse> source, IRepository<PostDetails> repository)
    {
        _redditSettings = redditSettings.Value;
        _source = source;
        _repository = repository;

        nextPageDictionary = new();
    }
    public void Execute()
    {
        while (true)
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
                        .ForEach(post => _repository.Save(post.Details));

                    nextPageDictionary[subReddit] = new PaginatedApiMetaData()
                    {
                        Category = subReddit,
                        NextPageId = response?.ListingInfo.NextPage ?? string.Empty,
                        PreviousPageId = response?.ListingInfo.PreviousPage ?? string.Empty
                    };
                });
        }
    }

    public Task ExecuteAsync()
    {
        // Due to time constraints, I am not implementing this.
        Execute();
        return Task.FromResult(true);
    }
}
