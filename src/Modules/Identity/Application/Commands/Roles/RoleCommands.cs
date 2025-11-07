namespace WebSearchIndexing.Modules.Identity.Application.Commands.Roles;

public sealed record CreateRoleCommand
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Permissions { get; init; } = string.Empty;
}

public sealed record UpdateRoleCommand
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Permissions { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

public sealed record DeleteRoleCommand
{
    public Guid Id { get; init; }
}
