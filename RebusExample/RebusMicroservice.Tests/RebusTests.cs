using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Routing.TypeBased;
using Rebus.Transport.InMem;
using Xunit;

namespace RebusMicroservice.Tests
{
    public class MicroserviceInMemoryRebusTest : IAsyncLifetime, IDisposable
    {
        private IHost _host = null!;
        private InMemNetwork _network = null!;
        private IBus _inputBus = null!;
        private bool _disposed;

        public async Task InitializeAsync()
        {
            _network = new InMemNetwork();

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    // Register Rebus with in-memory transport for input queue
                    services.AddRebus(configure => configure
                       .Transport(t => t.UseInMemoryTransport(_network, "input-queue"))
                       .Routing(r => r.TypeBased()
                           .Map<InputMessage>("input-queue")
                           .Map<ProcessedMessage>("output-queue"))
                   );

                    // Register your handler exactly as in your microservice
                    services.AddTransient<IHandleMessages<InputMessage>, MessageProcessor>();
                })
                .Build();

            await _host.StartAsync();

            // Get Rebus bus to send input messages
            _inputBus = _host.Services.GetRequiredService<IBus>();
        }

        [Fact]
        public async Task MicroserviceProcessesMessageAndSendsToOutputQueue()
        {
            ProcessedMessage? receivedProcessedMessage = null;

            // Setup separate activator and bus for output queue listening
            using var outputActivator = new BuiltinHandlerActivator();
            outputActivator.Handle<ProcessedMessage>(async msg =>
            {
                receivedProcessedMessage = msg;
                await Task.CompletedTask;
            });

            using var outputBus = Configure.With(outputActivator)
                .Transport(t => t.UseInMemoryTransport(_network, "output-queue"))
                .Start();

            // Send the input message to input queue, microservice handler should pick it up
            await _inputBus.Send(new InputMessage { Text = "hello" });

            // Wait some time for message processing
            await Task.Delay(500);

            Assert.NotNull(receivedProcessedMessage);
            Assert.Equal("hello-processed", receivedProcessedMessage!.Text);
        }

        public async Task DisposeAsync()
        {
            Dispose();
            if (_host is not null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}
