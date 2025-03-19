using Mango.Web.Models;

namespace Mango.Web.Service.IService;

public interface IProductService
{
	Task<ResponseDto?> GetAllProductsAsync();
	Task<ResponseDto?> GetProductAsync(int id);
	Task<ResponseDto?> CreateProductAsync(ProductDto product);
	Task<ResponseDto?> UpdateProductAsync(ProductDto product);
	Task<ResponseDto?> DeleteProductAsync(int id);
}
