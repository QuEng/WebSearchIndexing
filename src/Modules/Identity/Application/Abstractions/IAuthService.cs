using WebSearchIndexing.Modules.Identity.Application.Services;

namespace WebSearchIndexing.Modules.Identity.Application.Abstractions;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string email, string password, AuthenticationContext context, CancellationToken cancellationToken = default);
    Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<UserProfileResult> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UpdateProfileResult> UpdateUserProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<ChangePasswordResult> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
}
