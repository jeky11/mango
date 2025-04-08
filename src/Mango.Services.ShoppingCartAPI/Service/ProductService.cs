using Mango.Services.Infrastructure.Models.Dto;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Newtonsoft.Json;

namespace Mango.Services.ShoppingCartAPI.Service;

public class ProductService : IProductService
{
	private readonly IHttpClientFactory _httpClientFactory;

	public ProductService(IHttpClientFactory httpClientFactory)
	{
		_httpClientFactory = httpClientFactory;
	}

	public async Task<IEnumerable<ProductDto>> GetProducts()
	{
		var client = _httpClientFactory.CreateClient("Product");
		var response = await client.GetAsync("/api/product");
		var apiContent = await response.Content.ReadAsStringAsync();
		var responseDto = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
		if (responseDto?.Result == null || !responseDto.IsSuccess)
		{
			return new List<ProductDto>();
		}

		var resultStr = Convert.ToString(responseDto.Result);
		if (resultStr == null)
		{
			return new List<ProductDto>();
		}

		return JsonConvert.DeserializeObject<IEnumerable<ProductDto>>(resultStr) ?? new List<ProductDto>();
	}
}
