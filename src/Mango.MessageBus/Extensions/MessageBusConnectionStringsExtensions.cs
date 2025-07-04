namespace Mango.MessageBus.Extensions;

internal static class MessageBusConnectionStringsExtensions
{
	public static void Validate(this MessageBusConnectionStrings connectionStrings)
	{
		if (string.IsNullOrWhiteSpace(connectionStrings.MessageBusConnection) && string.IsNullOrWhiteSpace(connectionStrings.RabbitMQConnection))
		{
			throw new ArgumentException(
				$"You have to provide at least one connection string " +
				$"({nameof(MessageBusConnectionStrings.MessageBusConnection)} or {nameof(MessageBusConnectionStrings.RabbitMQConnection)})");
		}

		if (!string.IsNullOrWhiteSpace(connectionStrings.MessageBusConnection) && !string.IsNullOrWhiteSpace(connectionStrings.RabbitMQConnection))
		{
			throw new ArgumentException(
				$"You have to provide only one connection string " +
				$"({nameof(MessageBusConnectionStrings.MessageBusConnection)} or {nameof(MessageBusConnectionStrings.RabbitMQConnection)})");
		}
	}
}
