using System.Text;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mango.MessageBus.MessageBusConsumer;

public class AzureMessageBusConsumer(
	string connectionString,
	IServiceScopeFactory serviceScopeFactory,
	ILogger<AzureMessageBusConsumer> logger) : IMessageBusConsumer
{
	private readonly ServiceBusClient _client = new(connectionString);
	private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
	private readonly ILogger<AzureMessageBusConsumer> _logger = logger;
	private readonly List<ServiceBusProcessor> _processors = [];
	private readonly List<Type> _handlerTypes = [];

	public void RegisterHandler<THandler>() where THandler : IMessageHandler =>
		_handlerTypes.Add(typeof(THandler));

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		using var scope = _serviceScopeFactory.CreateScope();

		foreach (var handlerType in _handlerTypes)
		{
			var handler = (IMessageHandler)scope.ServiceProvider.GetRequiredService(handlerType);

			var processor = !string.IsNullOrWhiteSpace(handler.TopicName) && !string.IsNullOrWhiteSpace(handler.SubscriptionName)
				? _client.CreateProcessor(handler.TopicName, handler.SubscriptionName)
				: _client.CreateProcessor(handler.QueueName);

			processor.ProcessMessageAsync += async args => await HandleMessageAsync(args, handlerType);
			processor.ProcessErrorAsync += ErrorHandler;

			_processors.Add(processor);

			await processor.StartProcessingAsync(cancellationToken);
		}
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		foreach (var processor in _processors)
		{
			await processor.StopProcessingAsync(cancellationToken);
			await processor.DisposeAsync();
		}

		_processors.Clear();
	}

	private async Task HandleMessageAsync(ProcessMessageEventArgs arg, Type handlerType)
	{
		var message = arg.Message;
		var body = Encoding.UTF8.GetString(message.Body);

		try
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var handler = (IMessageHandler)scope.ServiceProvider.GetRequiredService(handlerType);

			await handler.HandleAsync(body);
			await arg.CompleteMessageAsync(arg.Message);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error while processing message with MessageId: {MessageId}", message.MessageId);

			if (message.DeliveryCount >= 3)
			{
				_logger.LogWarning(
					"Message with ID {MessageId} moved to dead-letter after {Count} attempts", message.MessageId, message.DeliveryCount);
				await arg.DeadLetterMessageAsync(message, "MaxRetryExceeded", "Exceeded retry count");
			}
			else
			{
				await arg.AbandonMessageAsync(message);
			}
		}
	}

	private Task ErrorHandler(ProcessErrorEventArgs arg)
	{
		_logger.LogError(arg.Exception, "Unexpected error while processing message");
		return Task.CompletedTask;
	}
}
