namespace WebSearchIndexing.Modules.Identity.Application.DTOs;

public sealed class InvitationResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<InvitationDto> Invitations { get; set; } = new();
}
