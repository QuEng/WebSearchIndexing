using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebSearchIndexing.BuildingBlocks.Abstractions.Entities;
using WebSearchIndexing.Domain.Repositories;

namespace WebSearchIndexing.Data.Repositories;

public class BaseRepository<T, TKey> : IRepository<T, TKey> where T : class, IEntity<TKey>
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
        var entity = await GetByIdAsync(id);
        if (entity is null)
        {
            return false;
        }

        context.Set<T>().Remove(entity);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> EntityExists(TKey id)
    {
        using var context = _factory.CreateDbContext();
        return await context.Set<T>().AnyAsync(item => EqualityComparer<TKey>.Default.Equals(item.Id, id));
    }

    public virtual async Task<List<T>> GetAllAsync()
    {
        using var context = _factory.CreateDbContext();
        return await context.Set<T>().ToListAsync();
    }

    public async Task<T?> GetByIdAsync(TKey id)
    {
        using var context = _factory.CreateDbContext();
        return await context.Set<T>().SingleOrDefaultAsync(item => EqualityComparer<TKey>.Default.Equals(item.Id, id));
    }
}

