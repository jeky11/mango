namespace Mango.MessageBus.Extensions;

internal static class MessageBusConnectionStringsExtensions
{
	public static void Validate(this MessageBusConnectionStrings connectionStrings)
	{
		if (string.IsNullOrWhiteSpace(connectionStrings.AzureServiceBusConnection) && string.IsNullOrWhiteSpace(connectionStrings.RabbitMQConnection))
		{
			throw new ArgumentException(
				$"You have to provide at least one connection string " +
				$"({nameof(MessageBusConnectionStrings.AzureServiceBusConnection)} or {nameof(MessageBusConnectionStrings.RabbitMQConnection)})");
		}

		if (!string.IsNullOrWhiteSpace(connectionStrings.AzureServiceBusConnection) && !string.IsNullOrWhiteSpace(connectionStrings.RabbitMQConnection))
		{
			throw new ArgumentException(
				$"You have to provide only one connection string " +
				$"({nameof(MessageBusConnectionStrings.AzureServiceBusConnection)} or {nameof(MessageBusConnectionStrings.RabbitMQConnection)})");
		}
	}
}
