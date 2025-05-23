using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.Extensions.Options;

namespace Mango.Web.Service;

public class OrderService(IBaseService baseService, IOptions<ServiceUrls> serviceUrls) : IOrderService
{
	private readonly IBaseService _baseService = baseService;
	private readonly Uri _baseUrl = new(serviceUrls.Value.OrderApi);

	public async Task<ResponseDto?> CreateOrderAsync(CartDto cartDto)
	{
		var responseDto = await _baseService.SendAsync(
			new RequestDto
			{
				ApiType = HttpMethod.Post,
				Url = new Uri(_baseUrl, "/api/order/create").ToString(),
				Data = cartDto
			});
		return responseDto;
	}

	public async Task<ResponseDto?> CreateStripeSessionAsync(StripeRequestDto stripeRequestDto)
	{
		var responseDto = await _baseService.SendAsync(
			new RequestDto
			{
				ApiType = HttpMethod.Post,
				Url = new Uri(_baseUrl, "/api/order/createStripeSession").ToString(),
				Data = stripeRequestDto
			});
		return responseDto;
	}

	public async Task<ResponseDto?> ValidateStripeSessionAsync(int orderHeaderId)
	{
		var responseDto = await _baseService.SendAsync(
			new RequestDto
			{
				ApiType = HttpMethod.Post,
				Url = new Uri(_baseUrl, "/api/order/validateStripeSession").ToString(),
				Data = orderHeaderId
			});
		return responseDto;
	}
}
