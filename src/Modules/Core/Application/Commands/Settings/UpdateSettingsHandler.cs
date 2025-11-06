using Microsoft.Extensions.Logging;
using WebSearchIndexing.Modules.Core.Domain.Repositories;
using WebSearchIndexing.Modules.Core.Application.DTOs;

namespace WebSearchIndexing.Modules.Core.Application.Commands.Settings;

public sealed class UpdateSettingsHandler
{
    private readonly ISettingsRepository _repository;
    private readonly ILogger<UpdateSettingsHandler> _logger;

    public UpdateSettingsHandler(ISettingsRepository repository, ILogger<UpdateSettingsHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<SettingsDto> HandleAsync(UpdateSettingsCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.RequestsPerDay <= 0)
        {
            throw new ArgumentException("RequestsPerDay must be greater than 0", nameof(command.RequestsPerDay));
        }

        var settings = await _repository.GetAsync();
        if (settings == null)
        {
            throw new InvalidOperationException("Settings not found.");
        }

        settings.RequestsPerDay = command.RequestsPerDay;
        if (command.IsEnabled.HasValue)
        {
            settings.IsEnabled = command.IsEnabled.Value;
        }

        var success = await _repository.UpdateAsync(settings);
        if (!success)
        {
            throw new InvalidOperationException("Failed to update settings.");
        }

        _logger.LogInformation("Settings were successfully updated.");

        return SettingsDto.FromDomain(settings);
    }
}
