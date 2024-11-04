using ReadIt.Models;

namespace ReadIt.Repositories;

public interface IRepository<T> where T: IRepositoryItem
{
    Task<T?> FindByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
    void Save(T entity);
    Task<bool> DeleteAsync(T entity);
}
public class InMemoryRepository<T> : IRepository<T> where T:IRepositoryItem
{
    // This class will serve as the in-memory variant of the repository. 
    // If I need to switch out for a true persistance layer I can create a new implementation.
    // As a concrete example, I could create a SQLGenericRepository<T> that works against a 
    // SQL Database. The only major updates would be to change the registration in Program.cs

    private List<T> _entities;
    
    public InMemoryRepository()
    {
        _entities = new List<T>();   
    }

    public Task<bool> DeleteAsync(T entity)
    {
        return Task.FromResult(_entities.Remove(entity));
    }

    public Task<T?> FindByIdAsync(string id)
    {
        return Task.FromResult(_entities.FirstOrDefault(e => e.Id == id));
    }

    public Task<IEnumerable<T>> GetAllAsync()
    {
        return Task.FromResult(_entities.AsEnumerable());
    }

    public void Save(T entity)
    {
        Console.WriteLine("Saving Post Information!");
        _entities.Add(entity);
    }
}
