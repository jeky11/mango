using Mango.Web.Models;
using Mango.Web.Models.Extensions;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Web.Controllers;

public class ProductController : Controller
{
	private readonly IProductService _productService;

	public ProductController(IProductService productService)
	{
		_productService = productService;
	}

	[HttpGet]
	public async Task<IActionResult> Index()
	{
		var response = await _productService.GetAllProductsAsync();
		if (!response.TryGetResult<List<ProductDto>>(out var products))
		{
			TempData["error"] = response?.Message ?? "Invalid products";
			return View(new List<ProductDto>());
		}

		return View(products);
	}

	[HttpGet]
	public IActionResult Create()
	{
		return View();
	}

	[HttpPost]
	public async Task<IActionResult> Create(ProductDto product)
	{
		if (!ModelState.IsValid)
		{
			return View(product);
		}

		var response = await _productService.CreateProductAsync(product);
		if (response is not {IsSuccess: true})
		{
			TempData["error"] = response?.Message;
			return View(product);
		}

		TempData["success"] = "Product created successfully";
		return RedirectToAction(nameof(Index));
	}

	[HttpGet]
	public async Task<IActionResult> Edit(int id)
	{
		var response = await _productService.GetProductAsync(id);
		if (!response.TryGetResult<ProductDto>(out var product))
		{
			TempData["error"] = response?.Message ?? "Invalid product";
			return RedirectToAction(nameof(Index));
		}

		return View(product);
	}

	[HttpPost]
	public async Task<IActionResult> Edit(ProductDto product)
	{
		if (!ModelState.IsValid)
		{
			return View(product);
		}

		var response = await _productService.UpdateProductAsync(product);
		if (response is not {IsSuccess: true})
		{
			TempData["error"] = response?.Message;
			return RedirectToAction(nameof(Index));
		}

		TempData["success"] = "Product updated successfully";
		return RedirectToAction(nameof(Index));
	}

	[HttpGet]
	public async Task<IActionResult> Delete(int id)
	{
		var response = await _productService.GetProductAsync(id);
		if (!response.TryGetResult<ProductDto>(out var product))
		{
			TempData["error"] = response?.Message ?? "Invalid product";
			return RedirectToAction(nameof(Index));
		}

		return View(product);
	}

	[HttpPost]
	public async Task<IActionResult> Delete(ProductDto product)
	{
		var response = await _productService.DeleteProductAsync(product.ProductId);
		if (response is not {IsSuccess: true})
		{
			TempData["error"] = response?.Message;
			return RedirectToAction(nameof(Index));
		}

		TempData["success"] = "Product deleted successfully";
		return RedirectToAction(nameof(Index));
	}
}
