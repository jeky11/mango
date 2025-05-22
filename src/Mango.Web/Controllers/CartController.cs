using Mango.Web.Models;
using Mango.Web.Models.Extensions;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Mango.Web.Controllers;

[Authorize]
public class CartController : Controller
{
	private readonly ICartService _cartService;
	private readonly IOrderService _orderService;

	public CartController(ICartService cartService, IOrderService orderService)
	{
		_cartService = cartService;
		_orderService = orderService;
	}

	[HttpGet]
	public async Task<IActionResult> Index()
	{
		return View(await LoadCartDtoBasedOnLoggedInUser());
	}

	[HttpGet]
	public async Task<IActionResult> Checkout()
	{
		return View(await LoadCartDtoBasedOnLoggedInUser());
	}

	[HttpPost]
	public async Task<IActionResult> Checkout(CartDto cartDto)
	{
		var cart = await LoadCartDtoBasedOnLoggedInUser();

		if (cartDto.CartHeader == null || cart.CartHeader == null)
		{
			TempData["error"] = "Invalid cart";
			return RedirectToAction(nameof(Index));
		}

		cart.CartHeader.Phone = cartDto.CartHeader.Phone;
		cart.CartHeader.Email = cartDto.CartHeader.Email;
		cart.CartHeader.FirstName = cartDto.CartHeader.FirstName;
		cart.CartHeader.LastName = cartDto.CartHeader.LastName;

		var response = await _orderService.CreateOrderAsync(cart);
		if (!response.TryGetResult<OrderHeaderDto>(out var orderHeaderDto))
		{
			TempData["error"] = response?.Message ?? "Invalid order";
			return RedirectToAction(nameof(Index));
		}

		var scheme = HttpContext.Request.Scheme;
		var host = HttpContext.Request.Host.Value;
		var stripeRequestDto = new StripeRequestDto
		{
			ApprovedUrl = Url.Action("Confirmation", "Cart", new {orderId = orderHeaderDto.OrderHeaderId}, scheme, host),
			CancelUrl = Url.Action("Checkout", "Cart", null, scheme, host),
			OrderHeader = orderHeaderDto,
		};

		var stripeResponse = await _orderService.CreateStripeSessionAsync(stripeRequestDto);
		if (!stripeResponse.TryGetResult<StripeRequestDto>(out var stripeResponseResult))
		{
			TempData["error"] = stripeResponse?.Message ?? "Invalid stripe response";
			return RedirectToAction(nameof(Index));
		}

		Response.Headers.Append("Location", stripeResponseResult.StripeSessionUrl);
		return new StatusCodeResult(303);
	}

	[HttpGet]
	public IActionResult Confirmation(int orderId)
	{
		return View(orderId);
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

	[HttpPost]
	public async Task<IActionResult> EmailCart(CartDto cartDto)
	{
		var cart = await LoadCartDtoBasedOnLoggedInUser();
		if (cart.CartHeader == null)
		{
			TempData["error"] = "Invalid cart";
			return RedirectToAction(nameof(Index));
		}

		cart.CartHeader.Email = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;

		var response = await _cartService.EmailCartAsync(cart);
		if (response?.Result == null || !response.IsSuccess)
		{
			TempData["error"] = response?.Message;
			return RedirectToAction(nameof(Index));
		}

		TempData["success"] = "Email will be sent shortly.";
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
		if (!response.TryGetResult<CartDto>(out var cartDto))
		{
			TempData["error"] = response?.Message ?? "Invalid cart";
			return new CartDto();
		}

		return cartDto;
	}
}
