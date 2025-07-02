using System;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Xunit;

namespace RebusMicroservice.Tests;
public class ServiceBusEmulatorTests(ServiceBusEmulatorFixture fixture) : IClassFixture<ServiceBusEmulatorFixture>
{
    private readonly ServiceBusEmulatorFixture _fixture = fixture;

    [Fact]
    public async Task Validate_MessageIsProcssed_Ok()
    {
        // Arrange
        var sender = _fixture.ServiceBusClient.CreateSender(_fixture.InputQueue);
        var receiver = _fixture.ServiceBusClient.CreateReceiver(_fixture.OutputQueue);

        var originalMessage = new { Name = "Alice" };
        var json = JsonSerializer.Serialize(originalMessage);

        await sender.SendMessageAsync(new ServiceBusMessage(json));

        // Act
        var receivedMessage = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(10));
        Assert.NotNull(receivedMessage);

        var body = receivedMessage.Body.ToString();

        // Assert augmented content
        Assert.Contains("Alice", body);
        Assert.Contains("augmented", body);

        await receiver.CompleteMessageAsync(receivedMessage);
    }
}
