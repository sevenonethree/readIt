using ReadIt.Models;

namespace ReadIt.Repositories;


public interface IPaginate<T>
{
    Task<PaginatedApiResponse<T>> GetPaginatedList(int pageNumber = 1, int resultsPerPage = 30);
}

