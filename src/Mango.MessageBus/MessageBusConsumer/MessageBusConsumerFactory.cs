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
		using var serviceScope = _serviceScopeFactory.CreateScope();
		var serviceProvider = serviceScope.ServiceProvider;

		return _useAzureMessageBus
			? CreateAzureMessageBusConsumer(serviceProvider)
			: CreateRabbitMQMessageBusConsumer(serviceProvider);
	}

	private RabbitMQMessageBusConsumer CreateRabbitMQMessageBusConsumer(IServiceProvider serviceProvider)
	{
		var handlers = serviceProvider.GetServices<IMessageHandler>();
		var logger = serviceProvider.GetRequiredService<ILogger<RabbitMQMessageBusConsumer>>();
		return new RabbitMQMessageBusConsumer(_rabbitMQConnectionString, handlers, logger);
	}

	private AzureMessageBusConsumer CreateAzureMessageBusConsumer(IServiceProvider serviceProvider)
	{
		var handlers = serviceProvider.GetServices<IMessageHandler>();
		var logger = serviceProvider.GetRequiredService<ILogger<AzureMessageBusConsumer>>();
		return new AzureMessageBusConsumer(_azureServiceBusConnectionString, handlers, logger);
	}
}
