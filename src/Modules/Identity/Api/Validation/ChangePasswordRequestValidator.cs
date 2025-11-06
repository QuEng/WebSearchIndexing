using FluentValidation;
using WebSearchIndexing.Modules.Identity.Api.Models;

namespace WebSearchIndexing.Modules.Identity.Api.Validation;

/// <summary>
/// Validator for change password requests with security policies
/// </summary>
public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("Current password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("New password is required")
            .MinimumLength(8)
            .WithMessage("New password must be at least 8 characters long")
            .MaximumLength(128)
            .WithMessage("New password must not exceed 128 characters")
            .Matches(@"[A-Z]")
            .WithMessage("New password must contain at least one uppercase letter")
            .Matches(@"[a-z]")
            .WithMessage("New password must contain at least one lowercase letter")
            .Matches(@"[0-9]")
            .WithMessage("New password must contain at least one number")
            .Matches(@"[\W_]")
            .WithMessage("New password must contain at least one special character")
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("New password must be different from current password");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithMessage("Password confirmation is required")
            .Equal(x => x.NewPassword)
            .WithMessage("Passwords do not match");
    }
}
