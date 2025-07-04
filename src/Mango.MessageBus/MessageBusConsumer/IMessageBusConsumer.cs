namespace Mango.MessageBus.MessageBusConsumer;

public interface IMessageBusConsumer
{
	Task StartAsync(CancellationToken cancellationToken = default);
	Task StopAsync(CancellationToken cancellationToken = default);
}
