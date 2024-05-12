namespace WebSearchIndexing.Domain.Entities;

public class Setting : BaseEntity<Guid>
{
    public Setting()
    {
        Id = Guid.NewGuid();
    }
    public int RequestsPerDay { get; set; }
    public bool IsEnabled { get; set; }
}