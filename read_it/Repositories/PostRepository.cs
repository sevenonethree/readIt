using ReadIt.Models;

namespace ReadIt.Repositories;

public class PostRepository : IPaginate<PostDetails>
{
    private readonly IRepository<PostDetails> _baseRepository;
    public PostRepository(IRepository<PostDetails> baseRepository) : base()
    {
        _baseRepository = baseRepository;
    }
    public async Task<PaginatedApiResponse<PostDetails>> GetPaginatedList(int pageNumber = 1, int resultsPerPage = 30)
    {
        // Prevent weird attempts to get negative number pages
        if (pageNumber <= 0) pageNumber = 1;

        if (resultsPerPage < 0) resultsPerPage = 0;

        // Not enough time to get this... well, sorted... but I would like to be able to pass in a custom sort
        var _entities = await _baseRepository.GetAllAsync();
        var sortedData = _entities
            .AsEnumerable()
            .OrderBy(x => x.Upvotes)
            .Skip((pageNumber - 1) * resultsPerPage)
            .Take(resultsPerPage);

        var response = new PaginatedApiResponse<PostDetails>()
        {
            PageNumber = pageNumber,
            TotalItemCount = _entities.Count(),
            Data = sortedData,
        };

        return response;
    }
}
