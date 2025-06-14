namespace Mango.Services.RewardAPI.Messaging;

public class AzureServiceBusHostedService(IAzureServiceBusConsumer serviceBusConsumer) : IHostedService
{
	private readonly IAzureServiceBusConsumer _serviceBusConsumer = serviceBusConsumer;

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		await _serviceBusConsumer.StartAsync();
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		await _serviceBusConsumer.StopAsync();
	}
}
