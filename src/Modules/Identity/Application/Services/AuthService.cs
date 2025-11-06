using WebSearchIndexing.Modules.Identity.Application.Abstractions;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;
using WebSearchIndexing.Modules.Identity.Domain.Entities;

namespace WebSearchIndexing.Modules.Identity.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly ITokenService _tokenService;

    public AuthService(
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _tokenService = tokenService;
    }

    public async Task<AuthResult> LoginAsync(string email, string password, AuthenticationContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                return new AuthResult(string.Empty, 0, DateTime.UtcNow, false, "Invalid email or password");
            }

            if (!VerifyPassword(password, user.PasswordHash))
            {
                return new AuthResult(string.Empty, 0, DateTime.UtcNow, false, "Invalid email or password");
            }

            if (!user.IsActive)
            {
                return new AuthResult(string.Empty, 0, DateTime.UtcNow, false, "Account is inactive");
            }

            // Update last login
            user.RecordLogin();
            await _userRepository.UpdateAsync(user, cancellationToken);

            return await _tokenService.AuthenticateAsync(user, context, cancellationToken: cancellationToken);
        }
        catch (Exception)
        {
            return new AuthResult(string.Empty, 0, DateTime.UtcNow, false, "An error occurred during login");
        }
    }

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if user already exists
            if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
            {
                return new RegisterResult(false, "User with this email already exists");
            }

            // Validate password strength (basic validation)
            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            {
                return new RegisterResult(false, "Password must be at least 6 characters long");
            }

            // Hash password
            var passwordHash = HashPassword(request.Password);

            // Create user
            var user = new User(request.Email, passwordHash, request.FirstName, request.LastName);

            await _userRepository.AddAsync(user, cancellationToken);

            return new RegisterResult(true, "User registered successfully", user.Id);
        }
        catch (Exception)
        {
            return new RegisterResult(false, "An error occurred during registration");
        }
    }

    public async Task<UserProfileResult> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return new UserProfileResult(false, "User not found");
            }

            return new UserProfileResult(true, "Success", new UserProfileData
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsActive = user.IsActive,
                IsEmailVerified = user.IsEmailVerified
            });
        }
        catch (Exception)
        {
            return new UserProfileResult(false, "An error occurred while retrieving user profile");
        }
    }

    public async Task<UpdateProfileResult> UpdateUserProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return new UpdateProfileResult(false, "User not found");
            }

            user.UpdateProfile(request.FirstName, request.LastName);
            await _userRepository.UpdateAsync(user, cancellationToken);

            return new UpdateProfileResult(true, "Profile updated successfully");
        }
        catch (Exception)
        {
            return new UpdateProfileResult(false, "An error occurred while updating profile");
        }
    }

    public async Task<ChangePasswordResult> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return new ChangePasswordResult(false, "User not found");
            }

            if (!VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                return new ChangePasswordResult(false, "Current password is incorrect");
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            {
                return new ChangePasswordResult(false, "New password must be at least 6 characters long");
            }

            var newPasswordHash = HashPassword(request.NewPassword);
            user.ChangePassword(newPasswordHash);
            await _userRepository.UpdateAsync(user, cancellationToken);

            // Revoke all existing tokens to force re-login
            await _tokenService.RevokeAllUserTokensAsync(userId, "Password changed", cancellationToken);

            return new ChangePasswordResult(true, "Password changed successfully");
        }
        catch (Exception)
        {
            return new ChangePasswordResult(false, "An error occurred while changing password");
        }
    }

    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
    }

    private static bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}

// Result models
public record RegisterResult(bool Success, string Message, Guid? UserId = null);
public record UserProfileResult(bool Success, string Message, UserProfileData? Profile = null);
public record UpdateProfileResult(bool Success, string Message);
public record ChangePasswordResult(bool Success, string Message);

public record RegisterRequest(string Email, string Password, string FirstName, string LastName);
public record UpdateProfileRequest(string FirstName, string LastName);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public class UserProfileData
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
}
