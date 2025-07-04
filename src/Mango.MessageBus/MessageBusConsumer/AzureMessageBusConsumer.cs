using System.Text;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Mango.MessageBus.MessageBusConsumer;

public class AzureMessageBusConsumer(
	string connectionString,
	IEnumerable<IMessageHandler> handlers,
	ILogger<AzureMessageBusConsumer> logger) : IMessageBusConsumer
{
	private readonly ServiceBusClient _client = new(connectionString);
	private readonly List<IMessageHandler> _handlers = handlers.ToList();
	private readonly ILogger<AzureMessageBusConsumer> _logger = logger;
	private readonly List<ServiceBusProcessor> _processors = [];

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		foreach (var handler in _handlers)
		{
			var processor = !string.IsNullOrWhiteSpace(handler.TopicName) && !string.IsNullOrWhiteSpace(handler.SubscriptionName)
				? _client.CreateProcessor(handler.TopicName, handler.SubscriptionName)
				: _client.CreateProcessor(handler.QueueName);

			processor.ProcessMessageAsync += async args => await HandleMessageAsync(args, handler.HandleAsync);
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

	private async Task HandleMessageAsync(ProcessMessageEventArgs arg, Func<string, Task> handler)
	{
		var message = arg.Message;
		var body = Encoding.UTF8.GetString(message.Body);

		try
		{
			await handler(body);
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
