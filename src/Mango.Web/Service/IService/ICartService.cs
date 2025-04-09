using Mango.Web.Models;

namespace Mango.Web.Service.IService;

public interface ICartService
{
	Task<ResponseDto?> GetCartByUserIdAsync(string userId);
	Task<ResponseDto?> UpsertCartAsync(CartDto cart);
	Task<ResponseDto?> RemoveFromCartAsync(int cartDetailsId);
	Task<ResponseDto?> ApplyCouponAsync(CartDto cart);
}
