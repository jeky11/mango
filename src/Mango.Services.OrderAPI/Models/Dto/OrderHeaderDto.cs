namespace Mango.Services.OrderAPI.Models.Dto;

public class OrderHeaderDto
{
	public int OrderHeaderId { get; set; }
	public string? UserId { get; set; }
	public string? CouponCode { get; set; }
	public decimal Discount { get; set; }
	public decimal OrderTotal { get; set; }

	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Email { get; set; }
	public string? Phone { get; set; }

	public DateTime OrderTime { get; set; }
	public Status? Status { get; set; }
	public string? PaymentIntentId { get; set; }
	public string? StripeSessionId { get; set; }
	public IEnumerable<OrderDetailsDto>? OrderDetails { get; set; }
}
