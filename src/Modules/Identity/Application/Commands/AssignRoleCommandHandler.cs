using MediatR;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;

namespace WebSearchIndexing.Modules.Identity.Application.Commands;

public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;

    public AssignRoleCommandHandler(
        IUserRepository userRepository,
        ITenantRepository tenantRepository)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
    }

    public async Task Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        // Validate user exists
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException($"User with ID '{request.UserId}' not found");
        }

        // Validate tenant exists
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant with ID '{request.TenantId}' not found");
        }

        // Assign role to user
        user.AddTenantRole(request.TenantId, request.Role);
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Also add user to tenant
        tenant.AddUser(request.UserId, request.Role);
        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
    }
}
