namespace Mango.Services.EmailAPI.Messaging;

public interface IRabbitMqConsumer
{
	Task StartAsync();
	Task StopAsync();
}
