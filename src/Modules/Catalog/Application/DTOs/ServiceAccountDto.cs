using WebSearchIndexing.Modules.Catalog.Domain;

namespace WebSearchIndexing.Modules.Catalog.Application.DTOs;

public sealed record ServiceAccountDto(
    Guid Id,
    string ProjectId,
    uint QuotaLimitPerDay,
    DateTime CreatedAt,
    DateTime? DeletedAt)
{
    public static ServiceAccountDto FromDomain(ServiceAccount serviceAccount)
    {
        ArgumentNullException.ThrowIfNull(serviceAccount);

        return new ServiceAccountDto(
            serviceAccount.Id,
            serviceAccount.ProjectId,
            serviceAccount.QuotaLimitPerDay,
            serviceAccount.CreatedAt,
            serviceAccount.DeletedAt);
    }
}
