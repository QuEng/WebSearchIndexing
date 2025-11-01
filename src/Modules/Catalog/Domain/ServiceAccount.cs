using System;
using WebSearchIndexing.BuildingBlocks.Abstractions.Entities;

namespace WebSearchIndexing.Modules.Catalog.Domain;

public sealed class ServiceAccount : IEntity<Guid>
{
    private ServiceAccount()
    {
        // For EF
    }

    public ServiceAccount(string projectId, string credentialsJson, uint quotaLimitPerDay)
    {
        Id = Guid.NewGuid();
        ArgumentException.ThrowIfNullOrWhiteSpace(projectId);
        ProjectId = projectId.Trim();
        CredentialsJson = credentialsJson ?? throw new ArgumentNullException(nameof(credentialsJson));
        QuotaLimitPerDay = quotaLimitPerDay;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string ProjectId { get; private set; } = string.Empty;
    public string CredentialsJson { get; private set; } = string.Empty;
    public uint QuotaLimitPerDay { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; private set; }

    private uint _quotaUsedInPeriod;

    public bool IsDeleted => DeletedAt.HasValue;
    public uint QuotaUsedInPeriod => _quotaUsedInPeriod;
    public uint RemainingQuota => QuotaLimitPerDay > _quotaUsedInPeriod ? QuotaLimitPerDay - _quotaUsedInPeriod : 0;

    public void UpdateQuota(uint quotaPerDay) => QuotaLimitPerDay = quotaPerDay;

    public void UpdateCredentials(string credentialsJson) => CredentialsJson = credentialsJson;

    public void LoadQuotaUsage(uint usedInPeriod) => _quotaUsedInPeriod = usedInPeriod;

    public bool CanHandle(uint requestsCount) => RemainingQuota >= requestsCount;

    public void MarkDeleted()
    {
        if (IsDeleted)
        {
            return;
        }

        DeletedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        DeletedAt = null;
    }
}
