namespace WebSearchIndexing.Modules.Core.Application.Commands.Settings;

public sealed record UpdateSettingsCommand(int RequestsPerDay, bool? IsEnabled = null);
