using MediatR;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;
using WebSearchIndexing.Modules.Identity.Application.DTOs;

namespace WebSearchIndexing.Modules.Identity.Application.Queries;

public class GetTenantQueryHandler : IRequestHandler<GetTenantQuery, TenantDto?>
{
    private readonly ITenantRepository _tenantRepository;

    public GetTenantQueryHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<TenantDto?> Handle(GetTenantQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        
        if (tenant == null)
            return null;

        return new TenantDto(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            tenant.Plan,
            tenant.CreatedAt,
            tenant.IsActive,
            tenant.OwnerId,
            tenant.MaxUsers,
            tenant.MaxDomains,
            tenant.DailyUrlQuota,
            tenant.DailyInspectionQuota);
    }
}
