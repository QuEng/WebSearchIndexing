namespace WebSearchIndexing.Contracts.Catalog;

public sealed record ServiceAccountDto(
    Guid Id,
    string ProjectId,
    uint QuotaLimitPerDay,
    uint QuotaUsedInPeriod,
    DateTime CreatedAt,
    DateTime? DeletedAt);
