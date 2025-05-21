namespace Mango.Services.EmailAPI.Models;

public class TopicAndQueueNames
{
	public required string EmailShoppingCartQueue { get; set; }
	public required string RegisterUserQueue { get; set; }
}
