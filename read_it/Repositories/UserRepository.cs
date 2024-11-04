using ReadIt.Models;

namespace ReadIt.Repositories;

public class UserRepository : IPaginate<ActiveRedditUser>
{
    private readonly IRepository<PostDetails> _repository;
    public UserRepository(IRepository<PostDetails> repository)
    {
        _repository = repository;
    }
    public async Task<PaginatedApiResponse<ActiveRedditUser>> GetPaginatedList(int pageNumber = 1, int resultsPerPage = 30)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (resultsPerPage <= 0) resultsPerPage = 30;

        var entities = await _repository.GetAllAsync();

        var users = entities
            .GroupBy(post => post.AuthorUserName)
            .Select(group => new ActiveRedditUser()
            {
                UserName = group.Key,
                PostCount = group.Count()
            })
            .OrderByDescending(x => x.PostCount);

        return new()
        {
            Data = users,
            PageNumber = pageNumber,
            TotalItemCount = entities.Count(),
        };
    }
}
