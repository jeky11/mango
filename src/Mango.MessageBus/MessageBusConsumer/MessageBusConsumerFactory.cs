using Mango.MessageBus.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Mango.MessageBus.MessageBusConsumer;

public interface IMessageBusConsumerFactory
{
	IMessageBusConsumer CreateMessageBusConsumer();
}

public class MessageBusConsumerFactory : IMessageBusConsumerFactory
{
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly ILogger<AzureMessageBusConsumer> _azureMessageBusLogger;
	private readonly ILogger<RabbitMQMessageBusConsumer> _rabbitMQMessageBusLogger;
	private readonly string _azureServiceBusConnectionString;
	private readonly string _rabbitMQConnectionString;
	private readonly bool _useAzureMessageBus;

	public MessageBusConsumerFactory(
		IOptions<MessageBusConnectionStrings> connectionStrings,
		IServiceScopeFactory serviceScopeFactory,
		ILogger<AzureMessageBusConsumer> azureMessageBusLogger,
		ILogger<RabbitMQMessageBusConsumer> rabbitMQMessageBusLogger)
	{
		connectionStrings.Value.Validate();

		_azureServiceBusConnectionString = connectionStrings.Value.AzureServiceBusConnection;
		_rabbitMQConnectionString = connectionStrings.Value.RabbitMQConnection;
		_useAzureMessageBus = !string.IsNullOrWhiteSpace(_azureServiceBusConnectionString);
		_serviceScopeFactory = serviceScopeFactory;
		_azureMessageBusLogger = azureMessageBusLogger;
		_rabbitMQMessageBusLogger = rabbitMQMessageBusLogger;
	}

	public IMessageBusConsumer CreateMessageBusConsumer() =>
		_useAzureMessageBus
			? new AzureMessageBusConsumer(_azureServiceBusConnectionString, _serviceScopeFactory, _azureMessageBusLogger)
			: new RabbitMQMessageBusConsumer(_rabbitMQConnectionString, _serviceScopeFactory, _rabbitMQMessageBusLogger);
}
