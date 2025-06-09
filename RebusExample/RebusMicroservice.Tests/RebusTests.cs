using System.Threading.Tasks;
using FluentAssertions;
using Rebus.Activation;
using Rebus.Config;
using Rebus.Transport.InMem;
using Xunit;

namespace RebusMicroservice.Tests
{
    public class MicroserviceInMemoryRebusTest(RebusHostFixture fixture) : IClassFixture<RebusHostFixture>

    {
        readonly RebusHostFixture _fixture = fixture;

        [Fact]
        public async Task ProcessesMessageAndSendsToOutputQueue()
        {
            // GIVEN
            // set up a Rebus listener on the output queue
            ProcessedMessage? receivedMessage = null;
            var outputActivator = new BuiltinHandlerActivator();
            outputActivator.Handle<ProcessedMessage>(msg => { receivedMessage = msg; return Task.CompletedTask; });

            var outputBus = Configure.With(outputActivator)
                .Transport(t => t.UseInMemoryTransport(_fixture.Network, "output-queue"))
                .Start();

            try
            {
                // WHEN
                // we send a message to the input queue
                await _fixture.HostBus.Send(new InputMessage { Text = "hello" });

                var timeout = Task.Delay(5000);

                while (receivedMessage == null && !timeout.IsCompleted)
                    await Task.Delay(50);

                // THEN
                receivedMessage.Should().NotBeNull("Expected a message to be received on the output queue, but none was received within the timeout period.");
                receivedMessage!.Text.Should().Be("hello-processed");
            }
            finally
            {
                outputBus.Dispose();
                outputActivator.Dispose();
            }
        }
    }
}
