using System.IdentityModel.Tokens.Jwt;
using Mango.Web.Models;
using Mango.Web.Models.Extensions;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Web.Controllers;

public class OrderController(IOrderService orderService) : Controller
{
	private readonly IOrderService _orderService = orderService;

	[HttpGet]
	public IActionResult Index()
	{
		return View();
	}

	[HttpGet]
	public async Task<IActionResult> OrderDetail(int orderId)
	{
		var userId = User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;

		var response = await _orderService.GetOrderAsync(orderId);
		if (!response.TryGetResult<OrderHeaderDto>(out var order))
		{
			TempData["error"] = response?.Message ?? "Order not found";
			return RedirectToAction(nameof(Index));
		}

		if (!User.IsInRole(nameof(Role.ADMIN)) && userId != order.UserId)
		{
			TempData["error"] = "Order not found";
			return RedirectToAction(nameof(Index));
		}

		return View(order);
	}

	[HttpGet]
	public async Task<IActionResult> GetAll()
	{
		var userId = User.IsInRole(nameof(Role.ADMIN))
			? string.Empty
			: User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;

		var response = await _orderService.GetAllOrdersAsync(userId);
		if (!response.TryGetResult<List<OrderHeaderDto>>(out var orders))
		{
			orders = [];
		}

		return Json(new {data = orders});
	}

	public IActionResult OrderReadyForPickup()
	{
		throw new NotImplementedException();
	}

	public IActionResult CompleteOrder()
	{
		throw new NotImplementedException();
	}

	public IActionResult CancelOrder()
	{
		throw new NotImplementedException();
	}
}
