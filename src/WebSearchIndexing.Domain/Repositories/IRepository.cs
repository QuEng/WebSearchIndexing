using WebSearchIndexing.BuildingBlocks.Abstractions.Entities;

namespace WebSearchIndexing.Domain.Repositories;

public interface IRepository<T, TKey> where T : class, IEntity<TKey>
{
    Task<bool> EntityExists(TKey id);
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(TKey id);
    Task<T> AddAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(TKey id);
}

