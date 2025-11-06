using MediatR;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;
using WebSearchIndexing.Modules.Identity.Application.DTOs;

namespace WebSearchIndexing.Modules.Identity.Application.Queries;

public class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, IEnumerable<UserRoleDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUserRolesQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserRoleDto>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user == null)
            return Enumerable.Empty<UserRoleDto>();

        var roles = new List<UserRoleDto>();

        // Add tenant roles
        foreach (var userTenant in user.UserTenants.Where(ut => ut.IsActive))
        {
            roles.Add(new UserRoleDto(
                user.Id,
                userTenant.TenantId,
                null,
                userTenant.Role,
                "Tenant",
                userTenant.IsActive,
                userTenant.CreatedAt));
        }

        // Add domain roles
        foreach (var userDomain in user.UserDomains.Where(ud => ud.IsActive))
        {
            roles.Add(new UserRoleDto(
                user.Id,
                null,
                userDomain.DomainId,
                userDomain.Role,
                "Domain",
                userDomain.IsActive,
                userDomain.CreatedAt));
        }

        return roles;
    }
}
