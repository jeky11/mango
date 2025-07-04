using Mango.MessageBus.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Mango.MessageBus.MessageBusConsumer;

public interface IMessageBusConsumerFactory
{
	IMessageBusConsumer CreateMessageBusConsumer();
}

public class MessageBusConsumerFactory : IMessageBusConsumerFactory
{
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly string _azureServiceBusConnectionString;
	private readonly string _rabbitMQConnectionString;
	private readonly bool _useAzureMessageBus;

	public MessageBusConsumerFactory(IOptions<MessageBusConnectionStrings> connectionStrings, IServiceScopeFactory serviceScopeFactory)
	{
		connectionStrings.Value.Validate();

		_azureServiceBusConnectionString = connectionStrings.Value.AzureServiceBusConnection;
		_rabbitMQConnectionString = connectionStrings.Value.RabbitMQConnection;
		_useAzureMessageBus = !string.IsNullOrWhiteSpace(_azureServiceBusConnectionString);
		_serviceScopeFactory = serviceScopeFactory;
	}

	public IMessageBusConsumer CreateMessageBusConsumer()
	{
		var handlers = _serviceScopeFactory.CreateScope().ServiceProvider.GetServices<IMessageHandler>();

		return _useAzureMessageBus
			? new AzureMessageBusConsumer(_azureServiceBusConnectionString, handlers)
			: new RabbitMQMessageBusConsumer(_rabbitMQConnectionString, handlers);
	}
}
