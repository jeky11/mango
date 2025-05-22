using System.Diagnostics;
using Mango.Web.Models;
using Mango.Web.Models.Extensions;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Mango.Web.Controllers;

public class HomeController : Controller
{
	private readonly IProductService _productService;
	private readonly ICartService _cartService;

	public HomeController(IProductService productService, ICartService cartService)
	{
		_productService = productService;
		_cartService = cartService;
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
	[Authorize]
	public async Task<IActionResult> Details(int id)
	{
		var response = await _productService.GetProductAsync(id);
		if (!response.TryGetResult<ProductDto>(out var product))
		{
			TempData["error"] = response?.Message ?? "Invalid product";
			return View(new ProductDto());
		}

		return View(product);
	}

	[HttpPost]
	[Authorize]
	public async Task<IActionResult> Details(ProductDto productDto)
	{
		var cartDetailsDto = new CartDetailsDto
		{
			Count = productDto.Count,
			ProductId = productDto.ProductId,
		};

		var cartDto = new CartDto
		{
			CartHeader = new CartHeaderDto {UserId = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value},
			CartDetails = new List<CartDetailsDto> {cartDetailsDto},
		};

		var response = await _cartService.UpsertCartAsync(cartDto);

		if (response?.Result == null || !response.IsSuccess)
		{
			TempData["error"] = response?.Message;
			return View(productDto);
		}

		var resultStr = Convert.ToString(response.Result);
		if (resultStr == null)
		{
			return BadRequest();
		}

		TempData["success"] = "Item has been added to the Shopping Cart";
		return RedirectToAction(nameof(Index));
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
