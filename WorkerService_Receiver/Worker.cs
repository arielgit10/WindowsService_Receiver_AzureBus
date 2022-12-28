using Azure.Messaging.ServiceBus;
using System.Text.Json;
using WorkerService_Receiver.Models;

namespace WorkerService_Receiver
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private ServiceBusClient client;
        private ServiceBusProcessor processor;
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ReadQueue();
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task ReadQueue()
        {
            var clientOptions = new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };

            client = new ServiceBusClient(AppSettings.QueueConnection, clientOptions);
            processor = client.CreateProcessor("cola1", new ServiceBusProcessorOptions());

            try
            {
                _logger.LogInformation("Reading Queue...", DateTimeOffset.Now);
                processor.ProcessMessageAsync += MessageHandler;
                processor.ProcessErrorAsync += ErrorHandler;

                await processor.StartProcessingAsync();

                Console.WriteLine("Wait for a minute and then press any key to end the processing");
                Console.ReadKey();
                Console.WriteLine("\nStopping the receiver...");
                await processor.StopProcessingAsync();
                Console.WriteLine("Stopped receiving messages");
            }
            finally
            {
                await processor.DisposeAsync();
                await client.DisposeAsync();
            }
        }

        async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            _logger.LogInformation("\nReceived at: {time}", DateTimeOffset.Now);
            _logger.LogInformation("Sending to database at: {time}", DateTimeOffset.Now);
            await SendToDatabase(body);
            await args.CompleteMessageAsync(args.Message);
        }
        Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
        private async Task SendToDatabase(string body)
        {
            List<AccountDto> accountsDto = JsonSerializer.Deserialize<List<AccountDto>>(body);
            IServerRepository dbServer = new ServerRepository();

            foreach (var accountDto in accountsDto)
            {
                await dbServer.PostAccount(new Account
                {
                    Alias = accountDto.Alias,
                    Balance = accountDto.Balance,
                    Cbu =accountDto.Cbu,
                }) ;
            }
        }
    }
}
