using Microsoft.Extensions.Logging;

namespace DurableTaskConsole;

public class TaskHubBackgroundWorker : IHostedService
{
    private readonly TaskHubWorker _hubWorker;
    private readonly ILogger<TaskHubBackgroundWorker> _logger;

    public TaskHubBackgroundWorker(TaskHubWorker hubWorker,
        ILogger<TaskHubBackgroundWorker> logger)
    {
        _hubWorker = hubWorker;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _hubWorker.orchestrationService.CreateIfNotExistsAsync();
        await _hubWorker.StartAsync().ConfigureAwait(false);
        
        _logger.LogInformation($"{nameof(TaskHubBackgroundWorker)} started");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        var cancel = Task.Delay(Timeout.Infinite, cancellationToken);
        await Task.WhenAny(_hubWorker.StopAsync(), cancel).ConfigureAwait(false);
        _logger.LogInformation($"{nameof(TaskHubBackgroundWorker)} stopped");
    }
}