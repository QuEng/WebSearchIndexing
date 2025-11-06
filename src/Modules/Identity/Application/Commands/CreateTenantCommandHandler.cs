using MediatR;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;
using WebSearchIndexing.Modules.Identity.Domain.Entities;

namespace WebSearchIndexing.Modules.Identity.Application.Commands;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Guid>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;

    public CreateTenantCommandHandler(
        ITenantRepository tenantRepository,
        IUserRepository userRepository)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
    }

    public async Task<Guid> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        // Validate owner exists
        var owner = await _userRepository.GetByIdAsync(request.OwnerId, cancellationToken);
        if (owner == null)
        {
            throw new InvalidOperationException($"Owner with ID '{request.OwnerId}' not found");
        }

        // Check if slug is already taken
        if (await _tenantRepository.ExistsBySlugAsync(request.Slug, cancellationToken))
        {
            throw new InvalidOperationException($"Tenant with slug '{request.Slug}' already exists");
        }

        // Create tenant
        var tenant = new Tenant(
            request.Name,
            request.Slug,
            request.OwnerId,
            request.Plan);

        // Add owner as Owner role
        tenant.AddUser(request.OwnerId, "Owner");

        await _tenantRepository.AddAsync(tenant, cancellationToken);

        return tenant.Id;
    }
}
