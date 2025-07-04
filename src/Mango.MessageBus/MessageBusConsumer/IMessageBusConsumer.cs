namespace Mango.MessageBus.MessageBusConsumer;

public interface IMessageBusConsumer
{
	void RegisterHandler<THandler>() where THandler : IMessageHandler;
	Task StartAsync(CancellationToken cancellationToken = default);
	Task StopAsync(CancellationToken cancellationToken = default);
}
