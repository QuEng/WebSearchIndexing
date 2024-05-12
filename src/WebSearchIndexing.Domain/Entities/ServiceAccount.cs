using System.ComponentModel.DataAnnotations.Schema;

namespace WebSearchIndexing.Domain.Entities;

public class ServiceAccount : BaseEntity<Guid>
{
    public ServiceAccount()
    {
        Id = Guid.NewGuid();
    }

    public string ProjectId { get; set; } = string.Empty;
    public string Json { get; set; }
    public uint QuotaLimitPerDay { get; set; } = 200;
    [NotMapped]
    public int QuotaLimitPerDayUsed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}