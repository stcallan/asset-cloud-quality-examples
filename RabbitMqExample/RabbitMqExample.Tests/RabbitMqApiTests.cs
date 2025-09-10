using FluentAssertions;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace RabbitMqExample.Tests;

public class RabbitMqApiTests : IClassFixture<RabbitMqTestContainerFixture>
{
    private readonly RabbitMqTestContainerFixture _fixture;
    private CustomWebApplicationFactory<Program> _webApplicationFactory;
    private HttpClient _client;

    public RabbitMqApiTests(RabbitMqTestContainerFixture fixture)
    {
        _fixture = fixture;
        _webApplicationFactory = new CustomWebApplicationFactory<Program>(_fixture);

        _client = _webApplicationFactory.CreateClient();
    }

    [Fact]
    public async Task SendMessage_ShouldPublishToQueue()
    {
        // Arrange
        var queueName = RabbitMqQueueNames.ApiQueue;
        var message = "Hello RabbitMQ Testcontainers!";

        // Act - call the API to send a message to the queue
        var response = await _client.PostAsJsonAsync("/api/messages", message);
        response.EnsureSuccessStatusCode();

        // Assert - consume the message directly from output queue
        var text = await _fixture.QueueClient.ConsumeMessageAsync(queueName);
        text.Should().Be(message);
    }

    [Fact]
    public async Task MessageProcessor_ShouldConsumeAndAugmentMessage()
    {
        var inputQueue = RabbitMqQueueNames.InputQueue;
        var outputQueue = RabbitMqQueueNames.OutputQueue;
        var originalMessage = "integration-test-message";

        // Send message to input queue
        await _fixture.QueueClient.PublishAsync(inputQueue, originalMessage);

        // Wait for the background service to process the message
        var outputMessage = await WaitForMessageAsync(outputQueue);

        outputMessage.Should().NotBeNull("The message processor should have published an augmented message to the output queue.");
        outputMessage.Should().Be($"{originalMessage}-augmented");
    }

    private async Task<string?> WaitForMessageAsync(string queueName, int maxAttempts = 10, int delayMs = 500)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            var message = await _fixture.QueueClient.ConsumeMessageAsync(queueName);
            if (message != null)
                return message;
            await Task.Delay(delayMs);
        }
        return null;
    }
}
