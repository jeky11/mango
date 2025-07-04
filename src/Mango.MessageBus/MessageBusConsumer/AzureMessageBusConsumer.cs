using System.Text;
using Azure.Messaging.ServiceBus;

namespace Mango.MessageBus.MessageBusConsumer;

public class AzureMessageBusConsumer(string connectionString, IEnumerable<IMessageBusHandler> handlers) : IMessageBusConsumer
{
	private readonly ServiceBusClient _client = new(connectionString);
	private readonly List<IMessageBusHandler> _handlers = handlers.ToList();
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

	private static async Task HandleMessageAsync(ProcessMessageEventArgs arg, Func<string, Task> handler)
	{
		var message = arg.Message;
		var body = Encoding.UTF8.GetString(message.Body);

		try
		{
			await handler(body);
			await arg.CompleteMessageAsync(arg.Message);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			throw;
		}
	}

	private static Task ErrorHandler(ProcessErrorEventArgs arg)
	{
		Console.WriteLine(arg.Exception.ToString());
		return Task.CompletedTask;
	}
}
