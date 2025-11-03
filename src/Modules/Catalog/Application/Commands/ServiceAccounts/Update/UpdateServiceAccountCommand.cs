namespace WebSearchIndexing.Modules.Catalog.Application.Commands.ServiceAccounts.Update;

public sealed record UpdateServiceAccountCommand(Guid Id, uint QuotaLimitPerDay);
