namespace Mango.Services.EmailAPI.Messaging;

public class RabbitMqHostedService(IRabbitMqConsumer rabbitMqConsumer) : IHostedService
{
	private readonly IRabbitMqConsumer _rabbitMqConsumer = rabbitMqConsumer;

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		await _rabbitMqConsumer.StartAsync();
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		await _rabbitMqConsumer.StopAsync();
	}
}
