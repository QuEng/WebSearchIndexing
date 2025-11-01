using WebSearchIndexing.BuildingBlocks.Abstractions.Entities;

namespace WebSearchIndexing.Domain.Entities;

public class BaseEntity<T> : IEntity<T>
{
    public T Id { get; set; } = default!;
}

