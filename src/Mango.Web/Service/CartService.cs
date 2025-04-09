using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.Extensions.Options;

namespace Mango.Web.Service;

public class CartService : ICartService
{
	private readonly IBaseService _baseService;
	private readonly Uri _baseUrl;

	public CartService(IBaseService baseService, IOptions<ServiceUrls> serviceUrls)
	{
		_baseService = baseService;
		_baseUrl = new Uri(serviceUrls.Value.ShoppingCartApi);
	}

	public async Task<ResponseDto?> GetCartByUserIdAsync(string userId)
	{
		var responseDto = await _baseService.SendAsync(
			new RequestDto
			{
				ApiType = HttpMethod.Get,
				Url = new Uri(_baseUrl, $"/api/cart/get/{userId}").ToString(),
			});
		return responseDto;
	}

	public async Task<ResponseDto?> UpsertCartAsync(CartDto cart)
	{
		var responseDto = await _baseService.SendAsync(
			new RequestDto
			{
				ApiType = HttpMethod.Post,
				Url = new Uri(_baseUrl, "/api/cart/upsert").ToString(),
				Data = cart
			});
		return responseDto;
	}

	public async Task<ResponseDto?> RemoveFromCartAsync(int cartDetailsId)
	{
		var responseDto = await _baseService.SendAsync(
			new RequestDto
			{
				ApiType = HttpMethod.Post,
				Url = new Uri(_baseUrl, "/api/cart/remove").ToString(),
				Data = cartDetailsId
			});
		return responseDto;
	}

	public async Task<ResponseDto?> ApplyCouponAsync(CartDto cart)
	{
		var responseDto = await _baseService.SendAsync(
			new RequestDto
			{
				ApiType = HttpMethod.Post,
				Url = new Uri(_baseUrl, "/api/cart/applyCoupon").ToString(),
				Data = cart
			});
		return responseDto;
	}
}
