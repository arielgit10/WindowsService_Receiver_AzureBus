using WorkerService_Receiver.Models;

namespace WorkerService_Receiver
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly TimeSpan _period;
        private readonly IServiceScopeFactory _factory;
        private int _executionCount = 0;
        public Worker(ILogger<Worker> logger, IServiceScopeFactory factory)
        {
            _logger = logger;
            _period = TimeSpan.FromMinutes(AppConfiguration.IntervalMinutes);
            _factory = factory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using PeriodicTimer timer = new PeriodicTimer(_period);
                while (!stoppingToken.IsCancellationRequested &&
                      await timer.WaitForNextTickAsync(stoppingToken))
                {
                try
                {
                    await using AsyncServiceScope asyncScope = _factory.CreateAsyncScope();
                    SampleService sampleService = asyncScope.ServiceProvider.GetRequiredService<SampleService>();
                    await sampleService.DoTaskAsync();

                    _executionCount++;
                    _logger.LogInformation($"Executed PeriodicHostedService - Count: {_executionCount}");
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(
                        $"Failed to execute PeriodicHostedService with exception message {ex.Message}.");
                }
            }
        }
    }
}