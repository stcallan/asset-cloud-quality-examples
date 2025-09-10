using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMqExample;
using System.Threading;
using System.Threading.Tasks;

public class RabbitMqMessageProcessor : BackgroundService
{
    private readonly RabbitMqQueueClient _queueClient;
    private readonly ILogger<RabbitMqMessageProcessor> _logger;

    public RabbitMqMessageProcessor(RabbitMqQueueClient queueClient, ILogger<RabbitMqMessageProcessor> logger)
    {
        _queueClient = queueClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var inputText = await _queueClient.ConsumeMessageAsync(RabbitMqQueueNames.InputQueue);
            if (inputText != null)
            {
                var augmentedText = $"{inputText}-augmented";
                _logger.LogInformation($"Received: {inputText}, Augmented: {augmentedText}");
                await _queueClient.PublishAsync(RabbitMqQueueNames.OutputQueue, augmentedText);
            }
            else
            {
                await Task.Delay(1000, stoppingToken); // Poll every second if no message
            }
        }
    }
}