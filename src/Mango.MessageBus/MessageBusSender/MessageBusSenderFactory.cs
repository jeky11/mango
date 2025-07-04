using Mango.MessageBus.Extensions;
using Microsoft.Extensions.Options;

namespace Mango.MessageBus.MessageBusSender;

public class MessageBusSenderFactory : IMessageBusSenderFactory
{
	private readonly string _azureServiceBusConnectionString;
	private readonly string _rabbitMQConnectionString;
	private readonly bool _useAzureMessageBus;

	public MessageBusSenderFactory(IOptions<MessageBusConnectionStrings> connectionStrings)
	{
		connectionStrings.Value.Validate();
		_azureServiceBusConnectionString = connectionStrings.Value.AzureServiceBusConnection;
		_rabbitMQConnectionString = connectionStrings.Value.RabbitMQConnection;
		_useAzureMessageBus = !string.IsNullOrWhiteSpace(_azureServiceBusConnectionString);
	}

	public IMessageBusSender CreateMessageBusSender() =>
		_useAzureMessageBus
			? new AzureMessageBusSender(_azureServiceBusConnectionString)
			: new RabbitMQMessageBusSender(_rabbitMQConnectionString);
}
