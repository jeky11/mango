using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.Extensions.Options;

namespace Mango.Web.Service;

public class CouponService : ICouponService
{
	private readonly IBaseService _baseService;
	private readonly Uri _baseUrl;

	public CouponService(IBaseService baseService, IOptions<ServiceUrls> serviceUrls)
	{
		_baseService = baseService;
		var host = new Uri(serviceUrls.Value.CouponApi);
		_baseUrl = new Uri(host, "/api/coupon");
	}

	public async Task<ResponseDto?> GetAllCouponsAsync()
	{
		var responseDto = await _baseService.SendAsync(
			new RequestDto
			{
				ApiType = HttpMethod.Get,
				Url = _baseUrl.ToString(),
			});
		return responseDto;
	}

	public async Task<ResponseDto?> GetCouponAsync(int id)
	{
		var responseDto = await _baseService.SendAsync(
			new RequestDto
			{
				ApiType = HttpMethod.Get,
				Url = new Uri(_baseUrl, $"/{id}").ToString(),
			});
		return responseDto;
	}

	public async Task<ResponseDto?> GetCouponAsync(string couponCode)
	{
		var responseDto = await _baseService.SendAsync(
			new RequestDto
			{
				ApiType = HttpMethod.Get,
				Url = new Uri(_baseUrl, $"/getByCode/{couponCode}").ToString(),
			});
		return responseDto;
	}

	public async Task<ResponseDto?> CreateCouponAsync(CouponDto coupon)
	{
		var responseDto = await _baseService.SendAsync(
			new RequestDto
			{
				ApiType = HttpMethod.Post,
				Url = _baseUrl.ToString(),
				Data = coupon
			});
		return responseDto;
	}

	public async Task<ResponseDto?> UpdateCouponAsync(CouponDto coupon)
	{
		var responseDto = await _baseService.SendAsync(
			new RequestDto
			{
				ApiType = HttpMethod.Put,
				Url = _baseUrl.ToString(),
				Data = coupon
			});
		return responseDto;
	}

	public async Task<ResponseDto?> DeleteCouponAsync(int id)
	{
		var responseDto = await _baseService.SendAsync(
			new RequestDto
			{
				ApiType = HttpMethod.Delete,
				Url = new Uri(_baseUrl, $"/{id}").ToString(),
			});
		return responseDto;
	}
}
