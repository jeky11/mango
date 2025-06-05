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
}
