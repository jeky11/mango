using Mango.MessageBus.MessageBusConsumer;
using Microsoft.Extensions.DependencyInjection;

namespace Mango.MessageBus.Extensions;

public static class ServiceProviderExtensions
{
	public static IServiceProvider RegisterMessageHandler<THandler>(this IServiceProvider serviceProvider) where THandler : IMessageHandler
	{
		serviceProvider.GetRequiredService<IMessageBusConsumer>().RegisterHandler<THandler>();
		return serviceProvider;
	}
}
