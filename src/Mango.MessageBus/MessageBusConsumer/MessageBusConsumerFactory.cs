using Mango.MessageBus.Extensions;
using Microsoft.Extensions.Options;

namespace Mango.MessageBus.MessageBusConsumer;

public interface IMessageBusConsumerFactory
{
	IMessageBusConsumer CreateMessageBusConsumer();
}

public class MessageBusConsumerFactory : IMessageBusConsumerFactory
{
	private readonly IEnumerable<IMessageBusHandler> _handlers;
	private readonly string _azureServiceBusConnectionString;
	private readonly string _rabbitMQConnectionString;
	private readonly bool _useAzureMessageBus;

	public MessageBusConsumerFactory(IOptions<MessageBusConnectionStrings> connectionStrings, IEnumerable<IMessageBusHandler> handlers)
	{
		connectionStrings.Value.Validate();

		_handlers = handlers;
		_azureServiceBusConnectionString = connectionStrings.Value.MessageBusConnection;
		_rabbitMQConnectionString = connectionStrings.Value.RabbitMQConnection;
		_useAzureMessageBus = !string.IsNullOrWhiteSpace(_azureServiceBusConnectionString);
	}

	public IMessageBusConsumer CreateMessageBusConsumer() =>
		_useAzureMessageBus
			? new AzureMessageBusConsumer(_azureServiceBusConnectionString, _handlers)
			: new RabbitMQMessageBusConsumer(_rabbitMQConnectionString, _handlers);
}
