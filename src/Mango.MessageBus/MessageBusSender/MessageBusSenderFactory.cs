using Microsoft.Extensions.Options;

namespace Mango.MessageBus.MessageBusSender;

public class MessageBusSenderFactory : IMessageBusSenderFactory
{
	private readonly string _azureServiceBusConnectionString;
	private readonly string _rabbitMQConnectionString;
	private readonly bool _useAzureMessageBus;

	public MessageBusSenderFactory(IOptions<MessageBusConnectionStrings> connectionStrings)
	{
		_azureServiceBusConnectionString = connectionStrings.Value.MessageBusConnection;
		_rabbitMQConnectionString = connectionStrings.Value.RabbitMQConnection;

		if (string.IsNullOrWhiteSpace(_azureServiceBusConnectionString) && string.IsNullOrWhiteSpace(_rabbitMQConnectionString))
		{
			throw new ArgumentException("You have to provide at least one connection string");
		}

		if (!string.IsNullOrWhiteSpace(_azureServiceBusConnectionString) && !string.IsNullOrWhiteSpace(_rabbitMQConnectionString))
		{
			throw new ArgumentException("You have to provide only one connection string");
		}

		_useAzureMessageBus = !string.IsNullOrWhiteSpace(_azureServiceBusConnectionString);
	}

	public IMessageBusSender CreateMessageBusSender() =>
		_useAzureMessageBus
			? new AzureMessageBusSender(_azureServiceBusConnectionString)
			: new RabbitMQMessageBusSender(_rabbitMQConnectionString);
}
