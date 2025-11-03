using WebSearchIndexing.Modules.Catalog.Domain;
using ContractDto = WebSearchIndexing.Contracts.Catalog.ServiceAccountDto;

namespace WebSearchIndexing.Modules.Catalog.Application.DTOs;

public sealed record ServiceAccountDto(
    Guid Id,
    string ProjectId,
    uint QuotaLimitPerDay,
    uint QuotaUsedInPeriod,
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
            serviceAccount.QuotaUsedInPeriod,
            serviceAccount.CreatedAt,
            serviceAccount.DeletedAt);
    }

    public ContractDto ToContract()
    {
        return new ContractDto(
            Id,
            ProjectId,
            QuotaLimitPerDay,
            QuotaUsedInPeriod,
            CreatedAt,
            DeletedAt);
    }

    public static ServiceAccountDto FromContract(ContractDto contractDto)
    {
        return new ServiceAccountDto(
            contractDto.Id,
            contractDto.ProjectId,
            contractDto.QuotaLimitPerDay,
            contractDto.QuotaUsedInPeriod,
            contractDto.CreatedAt,
            contractDto.DeletedAt);
    }
}
