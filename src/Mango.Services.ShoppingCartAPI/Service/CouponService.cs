using Mango.Services.Infrastructure.Models.Dto;
using Mango.Services.Infrastructure.Models.Dto.Extensions;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Newtonsoft.Json;

namespace Mango.Services.ShoppingCartAPI.Service;

public class CouponService(IHttpClientFactory httpClientFactory) : ICouponService
{
	private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

	public async Task<CouponDto?> GetCoupon(string couponCode)
	{
		var client = _httpClientFactory.CreateClient("Coupon");
		var response = await client.GetAsync($"/api/coupon/getByCode/{couponCode}");
		var apiContent = await response.Content.ReadAsStringAsync();

		var responseDto = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
		if (!responseDto.TryGetResult<CouponDto>(out var coupon))
		{
			return null;
		}

		return coupon;
	}
}
