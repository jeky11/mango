using Mango.Services.Infrastructure.Models.Dto;
using Mango.Services.Infrastructure.Models.Dto.Extensions;
using Mango.Services.OrderAPI.Models.Dto;
using Mango.Services.OrderAPI.Service.IService;
using Newtonsoft.Json;

namespace Mango.Services.OrderAPI.Service;

public class ProductService(IHttpClientFactory httpClientFactory) : IProductService
{
	private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

	public async Task<IEnumerable<ProductDto>> GetProducts()
	{
		var client = _httpClientFactory.CreateClient("Product");
		var response = await client.GetAsync("/api/product");
		var apiContent = await response.Content.ReadAsStringAsync();

		var responseDto = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
		if (!responseDto.TryGetResult<IEnumerable<ProductDto>>(out var products))
		{
			return new List<ProductDto>();
		}

		return products;
	}
}
