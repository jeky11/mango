using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json;

namespace Mango.Web.Controllers;

[Authorize]
public class CartController : Controller
{
	private readonly ICartService _cartService;

	public CartController(ICartService cartService)
	{
		_cartService = cartService;
	}

	[HttpGet]
	public async Task<IActionResult> Index()
	{
		return View(await LoadCartDtoBasedOnLoggedInUser());
	}

	[HttpGet]
	public async Task<IActionResult> Remove([FromQuery] int cartDetailsId)
	{
		var response = await _cartService.RemoveFromCartAsync(cartDetailsId);
		if (response?.Result == null || !response.IsSuccess)
		{
			TempData["error"] = response?.Message;
			return RedirectToAction(nameof(Index));
		}

		TempData["success"] = "Cart updated successfully";
		return RedirectToAction(nameof(Index));
	}

	[HttpPost]
	public async Task<IActionResult> ApplyCoupon(CartDto cartDto)
	{
		var response = await _cartService.ApplyCouponAsync(cartDto);
		if (response?.Result == null || !response.IsSuccess)
		{
			TempData["error"] = response?.Message;
			return RedirectToAction(nameof(Index));
		}

		TempData["success"] = "Cart updated successfully";
		return RedirectToAction(nameof(Index));
	}

	[HttpPost]
	public async Task<IActionResult> RemoveCoupon(CartDto cartDto)
	{
		if (cartDto.CartHeader == null)
		{
			TempData["error"] = "Invalid cart";
			return RedirectToAction(nameof(Index));
		}

		cartDto.CartHeader.CouponCode = string.Empty;
		var response = await _cartService.ApplyCouponAsync(cartDto);
		if (response?.Result == null || !response.IsSuccess)
		{
			TempData["error"] = response?.Message;
			return RedirectToAction(nameof(Index));
		}

		TempData["success"] = "Cart updated successfully";
		return RedirectToAction(nameof(Index));
	}

	private async Task<CartDto> LoadCartDtoBasedOnLoggedInUser()
	{
		var userId = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
		if (userId == null)
		{
			return new CartDto();
		}

		var response = await _cartService.GetCartByUserIdAsync(userId);
		if (response?.Result == null || !response.IsSuccess)
		{
			return new CartDto();
		}

		var responseStr = Convert.ToString(response.Result);
		if (responseStr == null)
		{
			return new CartDto();
		}

		var cartDto = JsonConvert.DeserializeObject<CartDto>(responseStr);
		return cartDto ?? new CartDto();
	}
}
