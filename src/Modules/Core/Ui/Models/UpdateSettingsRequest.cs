namespace WebSearchIndexing.Modules.Core.Ui.Models;

public record UpdateSettingsRequest(int RequestsPerDay, bool? IsEnabled = null);
