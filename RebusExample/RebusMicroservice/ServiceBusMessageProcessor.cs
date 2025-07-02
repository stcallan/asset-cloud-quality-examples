using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class ServiceBusMessageProcessor : BackgroundService
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusReceiver _receiver;
    private readonly ServiceBusSender _sender;
    private readonly ILogger<ServiceBusMessageProcessor> _logger;

    public ServiceBusMessageProcessor(IOptions<ServiceBusSettings> options, ILogger<ServiceBusMessageProcessor> logger)
    {
        var config = options.Value;

        _client = new ServiceBusClient(config.ConnectionString);

        _receiver = _client.CreateReceiver(config.InputQueue, new ServiceBusReceiverOptions
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock
        });

        _sender = _client.CreateSender(config.OutputQueue);

        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MessageProcessor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var message = await _receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5), stoppingToken);

                if (message is null)
                {
                    // No message received - loop again
                    continue;
                }

                var body = message.Body.ToString();
                _logger.LogInformation("Received message: {Body}", body);

                var data = JsonSerializer.Deserialize<Dictionary<string, object>>(body) ?? new();
                data["status"] = "augmented";

                var augmentedJson = JsonSerializer.Serialize(data);

                // Send augmented message
                await _sender.SendMessageAsync(new ServiceBusMessage(augmentedJson), stoppingToken);

                // Complete the original message
                await _receiver.CompleteMessageAsync(message, stoppingToken);

                _logger.LogInformation("Message processed and sent to output queue.");
            }
            catch (ServiceBusException sbEx) when (sbEx.Reason == ServiceBusFailureReason.ServiceTimeout)
            {
                // Timeout - just continue waiting for messages
                continue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message.");
            }
        }

        _logger.LogInformation("MessageProcessor stopping...");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _receiver.CloseAsync(cancellationToken);
        await _sender.CloseAsync(cancellationToken);
        await _client.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}
