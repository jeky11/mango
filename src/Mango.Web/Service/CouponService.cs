using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.Extensions.Options;
using HttpMethod = Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpMethod;

namespace Mango.Web.Service;

public class CouponService : ICouponService
{
	private readonly IBaseService _baseService;
	private readonly Uri _baseUrl;

	public CouponService(IBaseService baseService, IOptions<ServiceUrls> serviceUrls)
	{
		_baseService = baseService;
		_baseUrl = new Uri(serviceUrls.Value.CouponApi);
	}

	public async Task<ResponseDto?> GetAllCouponsAsync()
	{
		var responseDto = await _baseService.SendAsync(
			new RequestDto
			{
				ApiType = HttpMethod.Get,
				Url = new Uri(_baseUrl, "/api/coupon").ToString(),
			});
		return responseDto;
	}

	public async Task<ResponseDto?> GetCouponAsync(int id)
	{
		var responseDto = await _baseService.SendAsync(
			new RequestDto
			{
				ApiType = HttpMethod.Get,
				Url = new Uri(_baseUrl, $"/api/coupon/{id}").ToString(),
			});
		return responseDto;
	}

	public async Task<ResponseDto?> GetCouponAsync(string couponCode)
	{
		var responseDto = await _baseService.SendAsync(
			new RequestDto
			{
				ApiType = HttpMethod.Get,
				Url = new Uri(_baseUrl, $"/api/coupon/getByCode/{couponCode}").ToString(),
			});
		return responseDto;
	}

	public async Task<ResponseDto?> CreateCouponAsync(CouponDto coupon)
	{
		var responseDto = await _baseService.SendAsync(
			new RequestDto
			{
				ApiType = HttpMethod.Post,
				Url = new Uri(_baseUrl, "/api/coupon").ToString(),
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
				Url = new Uri(_baseUrl, "/api/coupon").ToString(),
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
				Url = new Uri(_baseUrl, $"/api/coupon/{id}").ToString(),
			});
		return responseDto;
	}
}
