using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System.Text.Json;
using WorkerService_Receiver.Models;

namespace WorkerService_Receiver
{
    class SampleService
    {
        private readonly ILogger<SampleService> _logger;
        private readonly IServerRepository _serverRepository;
        private readonly QueueClient _queueClient;

        public SampleService(ILogger<SampleService> logger, IServerRepository serverRepository)
        {
            _logger = logger;
            _serverRepository = serverRepository;
            _queueClient = new QueueClient(AppSettings.QueueConnection, "cola1");
        }

        public async Task DoTaskAsync()
        {
            _logger.LogInformation("Service running at: {time}", DateTimeOffset.Now); ;
            await ReadQueue();
        }

        private async Task ReadQueue()
        {
            if (_queueClient.Exists())
            {
                _logger.LogInformation("Reading Queue at: {time}", DateTimeOffset.Now);
                var queueMessages = await _queueClient.ReceiveMessagesAsync();
                if (queueMessages.Value != null)
                {
                    foreach (QueueMessage message in _queueClient.ReceiveMessages(maxMessages: 10).Value)
                    {                
                        var accountsDto = JsonSerializer.Deserialize<List<AccountDto>>(message.Body.ToString());
                        DisplayAccountInformation(accountsDto);
                        await SendToDatabase(accountsDto);
                        _queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                    }
                }
            }
        }

        private void DisplayAccountInformation(List<AccountDto> accountsDtos)
        {
            accountsDtos?.ForEach(account =>
            {
                _logger.LogInformation($"Account Information: \t{account.Cbu}" +
                    $"\t{account.Alias} \t{account.Balance}");
                _logger.LogInformation("\nMessage received at: {time}", DateTimeOffset.Now);
            });
        }
            
        private async Task SendToDatabase(List<AccountDto> accountsDtos)
        {
            foreach (var accountDto in accountsDtos)
            {
                _logger.LogInformation("Sending to database at: {time}", DateTimeOffset.Now); ;
                await _serverRepository.PostAccount(new Account
                {
                    Alias = accountDto.Alias,
                    Balance = accountDto.Balance,
                    Cbu = accountDto.Cbu,
                });
            }
        }
    }
}
