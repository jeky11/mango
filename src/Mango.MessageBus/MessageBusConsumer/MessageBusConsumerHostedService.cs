using Microsoft.Extensions.Hosting;

namespace Mango.MessageBus.MessageBusConsumer;

public class MessageBusConsumerHostedService(IMessageBusConsumer messageBusConsumer) : IHostedService
{
	private readonly IMessageBusConsumer _messageBusConsumer = messageBusConsumer;

	public async Task StartAsync(CancellationToken cancellationToken)
		=> await _messageBusConsumer.StartAsync(cancellationToken);

	public async Task StopAsync(CancellationToken cancellationToken)
		=> await _messageBusConsumer.StopAsync(cancellationToken);
}
