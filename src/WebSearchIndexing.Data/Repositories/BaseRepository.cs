namespace WebSearchIndexing.Data.Repositories;

public class BaseRepository<T, TKey> : IRepository<T, TKey> where T : BaseEntity<TKey>
{
    private readonly IDbContextFactory<IndexingDbContext> _factory;

    protected BaseRepository(IDbContextFactory<IndexingDbContext> factory)
    {
        _factory = factory;
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        using var context = _factory.CreateDbContext();
        await context.Set<T>().AddAsync(entity);
        await context.SaveChangesAsync();

        return entity;
    }
    public virtual async Task<bool> UpdateAsync(T entity)
    {
        if (await EntityExists(entity.Id) is false)
        {
            return false;
        }

        using var context = _factory.CreateDbContext();
        context.Set<T>().Update(entity);
        await context.SaveChangesAsync();

        return true;
    }

    public virtual async Task<bool> DeleteAsync(TKey id)
    {
        if (await EntityExists(id) is false)
        {
            return false;
        }

        using var context = _factory.CreateDbContext();
        context.Set<T>().Remove((await GetByIdAsync(id))!);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> EntityExists(TKey id)
    {
        using var context = _factory.CreateDbContext();
        return await context.Set<T>().AnyAsync(item => item.Id!.Equals(id));
    }

    public virtual async Task<List<T>> GetAllAsync()
    {
        using var context = _factory.CreateDbContext();
        return await context.Set<T>().ToListAsync();
    }

    public async Task<T?> GetByIdAsync(TKey id)
    {
        using var context = _factory.CreateDbContext();
        return await context.Set<T>().SingleOrDefaultAsync(item => item.Id!.Equals(id));
    }
}