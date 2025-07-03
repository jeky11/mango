namespace Mango.Services.EmailAPI.Messaging;

public interface IRabbitMqConsumer
{
	Task StartAsync(CancellationToken cancellationToken);
	Task StopAsync(CancellationToken cancellationToken);
}
