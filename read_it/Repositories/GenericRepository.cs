namespace ReadIt.Repositories;


public interface IRepository<T>
{
    Task<T?> FindByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
    void Save(T entity);
    Task<bool> DeleteAsync(T entity);
}

