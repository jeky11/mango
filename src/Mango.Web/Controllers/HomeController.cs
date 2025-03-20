using System.Diagnostics;
using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Mango.Web.Controllers;

public class HomeController : Controller
{
	private readonly IProductService _productService;

	public HomeController(IProductService productService)
	{
		_productService = productService;
	}

	[HttpGet]
	public async Task<IActionResult> Index()
	{
		var response = await _productService.GetAllProductsAsync();
		if (response?.Result == null || !response.IsSuccess)
		{
			TempData["error"] = response?.Message;
			return View(new List<ProductDto>());
		}

		var resultStr = Convert.ToString(response.Result);
		if (resultStr == null)
		{
			return BadRequest();
		}

		var products = JsonConvert.DeserializeObject<List<ProductDto>>(resultStr);
		return View(products);
	}

	[HttpGet]
	[Authorize]
	public async Task<IActionResult> Details(int id)
	{
		var response = await _productService.GetProductAsync(id);
		if (response?.Result == null || !response.IsSuccess)
		{
			TempData["error"] = response?.Message;
			return View(new ProductDto());
		}

		var resultStr = Convert.ToString(response.Result);
		if (resultStr == null)
		{
			return BadRequest();
		}

		var product = JsonConvert.DeserializeObject<ProductDto>(resultStr);
		return View(product);
	}

	public IActionResult Privacy()
	{
		return View();
	}

	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Error()
	{
		return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
	}
}
