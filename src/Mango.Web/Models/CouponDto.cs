namespace Mango.Web.Models;

public class CouponDto
{
	public int CouponId { get; set; }
	public required string CouponCode { get; set; }
	public decimal DiscountAmount { get; set; }
	public int MinAmount { get; set; }
}
