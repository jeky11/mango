using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Mango.MessageBus.MessageBusConsumer;

public class RabbitMQMessageBusConsumer(
	string connectionString,
	IEnumerable<IMessageHandler> handlers,
	ILogger<RabbitMQMessageBusConsumer> logger) : IMessageBusConsumer
{
	private readonly Uri _connectionString = new(connectionString);
	private readonly List<IMessageHandler> _handlers = handlers.ToList();
	private readonly ILogger<RabbitMQMessageBusConsumer> _logger = logger;
	private IConnection? _connection;
	private readonly List<IChannel> _channels = [];

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		var factory = new ConnectionFactory {Uri = _connectionString};
		_connection = await factory.CreateConnectionAsync(cancellationToken);

		foreach (var handler in _handlers)
		{
			var channel = !string.IsNullOrWhiteSpace(handler.TopicName) && !string.IsNullOrWhiteSpace(handler.SubscriptionName)
				? await RegisterHandlerAsync(_connection, handler.TopicName, handler.SubscriptionName, handler.HandleAsync, cancellationToken)
				: await RegisterHandlerAsync(_connection, handler.QueueName, handler.HandleAsync, cancellationToken);

			_channels.Add(channel);
		}
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		foreach (var channel in _channels)
		{
			await channel.DisposeAsync();
		}

		_channels.Clear();

		if (_connection != null)
		{
			await _connection.DisposeAsync();
		}
	}

	private async Task<IChannel> RegisterHandlerAsync(
		IConnection connection,
		string queueName,
		Func<string, Task> handler,
		CancellationToken cancellationToken)
	{
		var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
		await channel.QueueDeclareAsync(queueName, false, false, false, cancellationToken: cancellationToken);

		var consumer = new AsyncEventingBasicConsumer(channel);
		consumer.ReceivedAsync += async (_, eventArgs) => await HandleMessageAsync(channel, eventArgs, handler);

		await channel.BasicConsumeAsync(queueName, false, consumer, cancellationToken);

		return channel;
	}

	private async Task<IChannel> RegisterHandlerAsync(
		IConnection connection,
		string topicName,
		string subscriptionName,
		Func<string, Task> handler,
		CancellationToken cancellationToken)
	{
		var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
		await channel.QueueDeclareAsync(subscriptionName, false, false, false, cancellationToken: cancellationToken);

		if (!string.IsNullOrEmpty(topicName))
		{
			await channel.ExchangeDeclareAsync(topicName, ExchangeType.Fanout, false, false, cancellationToken: cancellationToken);
			await channel.QueueBindAsync(subscriptionName, topicName, "", cancellationToken: cancellationToken);
		}

		var consumer = new AsyncEventingBasicConsumer(channel);
		consumer.ReceivedAsync += async (_, eventArgs) => await HandleMessageAsync(channel, eventArgs, handler);

		await channel.BasicConsumeAsync(subscriptionName, false, consumer, cancellationToken);

		return channel;
	}

	private async Task HandleMessageAsync(IChannel channel, BasicDeliverEventArgs eventArgs, Func<string, Task> handler)
	{
		var body = Encoding.UTF8.GetString(eventArgs.Body.Span);
		try
		{
			await handler(body);
			await channel.BasicAckAsync(eventArgs.DeliveryTag, false);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error while processing message");
		}
	}
}
