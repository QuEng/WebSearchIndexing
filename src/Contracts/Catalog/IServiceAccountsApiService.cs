namespace WebSearchIndexing.Contracts.Catalog;

public interface IServiceAccountsApiService
{
    Task<IEnumerable<ServiceAccountDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ServiceAccountDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceAccountDto> AddAsync(AddServiceAccountRequest request, CancellationToken cancellationToken = default);
    Task<ServiceAccountDto> UpdateAsync(Guid id, UpdateServiceAccountRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string projectId, CancellationToken cancellationToken = default);
}

public record AddServiceAccountRequest(string ProjectId, string KeyFilePath, uint QuotaLimitPerDay);
public record UpdateServiceAccountRequest(uint QuotaLimitPerDay);
