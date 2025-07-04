namespace Mango.MessageBus;

public class MessageBusConnectionStrings
{
	public required string AzureServiceBusConnection { get; set; }
	public required string RabbitMQConnection { get; set; }
}
