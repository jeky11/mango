namespace Mango.Services.OrderAPI.Models;

public enum Status
{
	Pending,
	Approved,
	ReadyForPickup,
	Completed,
	Refunded,
	Cancelled,
}
