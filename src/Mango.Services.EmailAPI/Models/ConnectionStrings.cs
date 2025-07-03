namespace Mango.Services.EmailAPI.Models;

public class ConnectionStrings
{
	public required string DefaultConnection { get; set; }
	public required string MessageBusConnection { get; set; }
	public required string RabbitMQConnection { get; set; }
}
