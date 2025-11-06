namespace WebSearchIndexing.Modules.Identity.Application.DTOs;

public sealed class InvitationDetailsResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public InvitationDto? Invitation { get; set; }
}
