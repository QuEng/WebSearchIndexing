namespace WebSearchIndexing.BuildingBlocks.Abstractions.Entities;

public interface IEntity<TKey>
{
    TKey Id { get; }
}
