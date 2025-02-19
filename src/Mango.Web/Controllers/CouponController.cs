using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Mango.Web.Controllers;

public class CouponController : Controller
{
	private readonly ICouponService _couponService;

	public CouponController(ICouponService couponService)
	{
		_couponService = couponService;
	}

	public async Task<IActionResult> Index()
	{
		var response = await _couponService.GetAllCouponsAsync();
		if (response?.Result == null)
		{
			return BadRequest();
		}

		var resultStr = Convert.ToString(response.Result);
		if (resultStr == null)
		{
			return BadRequest();
		}

		var coupons = JsonConvert.DeserializeObject<List<CouponDto>>(resultStr);
		return View(coupons);
	}
}
