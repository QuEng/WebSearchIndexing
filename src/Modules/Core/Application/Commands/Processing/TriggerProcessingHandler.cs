using Microsoft.Extensions.Logging;
using WebSearchIndexing.Modules.Core.Application.BackgroundJobs;

namespace WebSearchIndexing.Modules.Core.Application.Commands.Processing;

public sealed class TriggerProcessingHandler
{
    private readonly IScopedRequestSendingService _requestSendingService;
    private readonly ILogger<TriggerProcessingHandler> _logger;

    public TriggerProcessingHandler(IScopedRequestSendingService requestSendingService, ILogger<TriggerProcessingHandler> logger)
    {
        _requestSendingService = requestSendingService;
        _logger = logger;
    }

    public async Task HandleAsync(TriggerProcessingCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        _logger.LogInformation("Triggering URL processing from API request");

        // Start processing in background as in the original implementation
        _ = Task.Run(async () => await _requestSendingService.DoWork(CancellationToken.None));

        _logger.LogInformation("URL processing triggered successfully");
    }
}
