using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using WebSearchIndexing.Modules.Identity.Application.Services;
using WebSearchIndexing.Modules.Identity.Domain.Repositories;
using WebSearchIndexing.Modules.Identity.Domain.Entities;

namespace WebSearchIndexing.Modules.Identity.Api.Endpoints;

public static class EmailVerificationEndpoints
{
    public static IEndpointRouteBuilder MapEmailVerificationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/email-verification")
            .WithTags("Email Verification");

        group.MapPost("/send", SendVerificationEmailAsync)
            .WithName("SendVerificationEmail")
            .WithDescription("Sends email verification link to user");

        group.MapPost("/verify", VerifyEmailAsync)
            .WithName("VerifyEmail")
            .WithDescription("Verifies user email with token");

        return endpoints;
    }

    private static async Task<IResult> SendVerificationEmailAsync(
        SendVerificationEmailRequest request,
        IEmailVerificationService emailVerificationService,
        CancellationToken cancellationToken)
    {
        try
        {
            await emailVerificationService.SendVerificationEmailAsync(request.UserId, cancellationToken);
            return Results.Ok(new { Message = "Verification email sent successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error sending verification email",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> VerifyEmailAsync(
        VerifyEmailRequest request,
        IEmailVerificationService emailVerificationService,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await emailVerificationService.VerifyEmailAsync(request.Token, cancellationToken);
            
            if (result)
            {
                return Results.Ok(new { Message = "Email verified successfully" });
            }
            
            return Results.BadRequest(new { Error = "Invalid or expired verification token" });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error verifying email",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }    
}

public sealed record SendVerificationEmailRequest(Guid UserId);
public sealed record VerifyEmailRequest(string Token);
