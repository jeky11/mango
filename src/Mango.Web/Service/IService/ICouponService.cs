using Mango.Web.Models;

namespace Mango.Web.Service.IService;

public interface ICouponService
{
	Task<ResponseDto?> GetAllCouponsAsync();
	Task<ResponseDto?> GetCouponAsync(int id);
	Task<ResponseDto?> GetCouponAsync(string couponCode);
	Task<ResponseDto?> CreateCouponAsync(CouponDto coupon);
	Task<ResponseDto?> UpdateCouponAsync(CouponDto coupon);
	Task<ResponseDto?> DeleteCouponAsync(int id);
}
