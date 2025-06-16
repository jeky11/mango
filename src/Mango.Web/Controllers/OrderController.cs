using System.IdentityModel.Tokens.Jwt;
using Mango.Web.Models;
using Mango.Web.Models.Extensions;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Web.Controllers;

[Authorize]
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
	public async Task<IActionResult> GetAll(string status)
	{
		var userId = User.IsInRole(nameof(Role.ADMIN))
			? string.Empty
			: User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;

		var response = await _orderService.GetAllOrdersAsync(userId);
		if (!response.TryGetResult<List<OrderHeaderDto>>(out var orders))
		{
			orders = [];
		}

		orders = status switch
		{
			"approved" => orders.Where(x => x.Status == Status.Approved).ToList(),
			"readyforpickup" => orders.Where(x => x.Status == Status.ReadyForPickup).ToList(),
			"cancelled" => orders.Where(x => x.Status is Status.Cancelled or Status.Refunded).ToList(),
			_ => orders
		};

		return Json(new {data = orders});
	}

	[HttpPost("orderReadyForPickup")]
	public async Task<IActionResult> OrderReadyForPickup(int orderId)
	{
		var response = await _orderService.UpdateOrderStatusAsync(orderId, Status.ReadyForPickup);
		if (response is not {IsSuccess: true})
		{
			TempData["error"] = response?.Message ?? "Cannot update order status";
		}
		else
		{
			TempData["success"] = "Status successfully updated";
		}

		return RedirectToAction(nameof(OrderDetail), new {orderId});
	}

	[HttpPost("completeOrder")]
	public async Task<IActionResult> CompleteOrder(int orderId)
	{
		var response = await _orderService.UpdateOrderStatusAsync(orderId, Status.Completed);
		if (response is not {IsSuccess: true})
		{
			TempData["error"] = response?.Message ?? "Cannot update order status";
		}
		else
		{
			TempData["success"] = "Status successfully updated";
		}

		return RedirectToAction(nameof(OrderDetail), new {orderId});
	}

	[HttpPost("cancelOrder")]
	public async Task<IActionResult> CancelOrder(int orderId)
	{
		var response = await _orderService.UpdateOrderStatusAsync(orderId, Status.Cancelled);
		if (response is not {IsSuccess: true})
		{
			TempData["error"] = response?.Message ?? "Cannot update order status";
		}
		else
		{
			TempData["success"] = "Status successfully updated";
		}

		return RedirectToAction(nameof(OrderDetail), new {orderId});
	}
}
