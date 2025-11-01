namespace WebSearchIndexing.Modules.Catalog.Application.Commands.ServiceAccounts;

public sealed record AddServiceAccountCommand(
    string ProjectId,
    string CredentialsJson,
    uint QuotaLimitPerDay);
