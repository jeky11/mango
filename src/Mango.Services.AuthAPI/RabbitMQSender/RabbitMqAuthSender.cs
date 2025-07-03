using System.Collections.Concurrent;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Mango.Services.AuthAPI.RabbitMQSender;

public class RabbitMqAuthSender : IRabbitMqAuthSender, IAsyncDisposable
{
	private readonly IConnection _connection;
	private readonly ConcurrentDictionary<string, IChannel> _channels = new();

	public RabbitMqAuthSender(string connectionString)
	{
		var factory = new ConnectionFactory {Uri = new Uri(connectionString)};
		_connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
	}

	public async Task PublishMessageAsync(object message, string queueName, CancellationToken cancellationToken = default)
	{
		var channel = _channels.GetOrAdd(
			queueName, qn =>
			{
				var ch = _connection.CreateChannelAsync(cancellationToken: cancellationToken).GetAwaiter().GetResult();
				ch.QueueDeclareAsync(qn, false, false, false, cancellationToken: cancellationToken).GetAwaiter().GetResult();
				return ch;
			});

		var jsonMessage = JsonConvert.SerializeObject(message);
		var body = Encoding.UTF8.GetBytes(jsonMessage);
		await channel.BasicPublishAsync("", queueName, body, cancellationToken);
	}

	public async ValueTask DisposeAsync()
	{
		foreach (var channel in _channels)
		{
			await channel.Value.DisposeAsync();
			_channels.TryRemove(channel);
		}

		await _connection.DisposeAsync();

		GC.SuppressFinalize(this);
	}
}
