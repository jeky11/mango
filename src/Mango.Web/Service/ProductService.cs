using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.Extensions.Options;

namespace Mango.Web.Service;

public class ProductService : IProductService
{
	private readonly IBaseService _baseService;
	private readonly Uri _baseUrl;

	public ProductService(IBaseService baseService, IOptions<ServiceUrls> serviceUrls)
	{
		_baseService = baseService;
		_baseUrl = new Uri(serviceUrls.Value.ProductApi);
	}

	public async Task<ResponseDto?> GetAllProductsAsync()
	{
		var responseDto = await _baseService.SendAsync(
			new RequestDto
			{
				ApiType = HttpMethod.Get,
				Url = new Uri(_baseUrl, "/api/product").ToString(),
			});
		return responseDto;
	}

	public async Task<ResponseDto?> GetProductAsync(int id)
	{
		var responseDto = await _baseService.SendAsync(
			new RequestDto
			{
				ApiType = HttpMethod.Get,
				Url = new Uri(_baseUrl, $"/api/product/{id}").ToString(),
			});
		return responseDto;
	}

	public async Task<ResponseDto?> CreateProductAsync(ProductDto product)
	{
		var responseDto = await _baseService.SendAsync(
			new RequestDto
			{
				ApiType = HttpMethod.Post,
				Url = new Uri(_baseUrl, "/api/product").ToString(),
				Data = product
			});
		return responseDto;
	}

	public async Task<ResponseDto?> UpdateProductAsync(ProductDto product)
	{
		var responseDto = await _baseService.SendAsync(
			new RequestDto
			{
				ApiType = HttpMethod.Put,
				Url = new Uri(_baseUrl, "/api/product").ToString(),
				Data = product
			});
		return responseDto;
	}

	public async Task<ResponseDto?> DeleteProductAsync(int id)
	{
		var responseDto = await _baseService.SendAsync(
			new RequestDto
			{
				ApiType = HttpMethod.Delete,
				Url = new Uri(_baseUrl, $"/api/product/{id}").ToString(),
			});
		return responseDto;
	}
}
