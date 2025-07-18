using System.Threading.Channels;

namespace Lisa.Services;

public class BackgroundJobService(IServiceProvider serviceProvider, ILogger<BackgroundJobService> logger)
    : BackgroundService
{
    private readonly Channel<Func<IServiceProvider, CancellationToken, Task>> _channel =
        Channel.CreateUnbounded<Func<IServiceProvider, CancellationToken, Task>>();

    public async Task QueueJobAsync(Func<IServiceProvider, CancellationToken, Task> job)
    {
        await _channel.Writer.WriteAsync(job);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                await job(scope.ServiceProvider, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing background job");
            }
        }
    }
}
