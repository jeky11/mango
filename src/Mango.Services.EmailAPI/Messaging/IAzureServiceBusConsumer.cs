namespace Mango.Services.EmailAPI.Messaging;

public interface IAzureServiceBusConsumer
{
	Task StartAsync(CancellationToken cancellationToken);
	Task StopAsync(CancellationToken cancellationToken);
}
