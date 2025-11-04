using WebSearchIndexing.BuildingBlocks.Abstractions.Entities;

namespace WebSearchIndexing.Modules.Core.Domain.Entities;

public sealed class Settings : IEntity<Guid>
{
    public const string DefaultKey = "core.default";
    public static readonly Guid DefaultTenantId = Guid.Empty;

    private Settings()
    {
        // For EF
    }

    private Settings(Guid tenantId, string key)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        Key = key;
    }

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public string Key { get; private set; } = DefaultKey;

    public int RequestsPerDay { get; set; }

    public bool IsEnabled { get; set; }

    public static Settings CreateDefault(Guid? tenantId = null, string? key = null)
    {
        var normalizedTenantId = tenantId ?? DefaultTenantId;
        var normalizedKey = string.IsNullOrWhiteSpace(key) ? DefaultKey : key.Trim();

        return new Settings(normalizedTenantId, normalizedKey)
        {
            RequestsPerDay = 0,
            IsEnabled = false
        };
    }
}

