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

	[HttpGet]
	public async Task<IActionResult> Index()
	{
		var response = await _couponService.GetAllCouponsAsync();
		if (response?.Result == null || !response.IsSuccess)
		{
			TempData["error"] = response?.Message;
			return View(new List<CouponDto>());
		}

		var resultStr = Convert.ToString(response.Result);
		if (resultStr == null)
		{
			return BadRequest();
		}

		var coupons = JsonConvert.DeserializeObject<List<CouponDto>>(resultStr);
		return View(coupons);
	}

	[HttpGet]
	public IActionResult Create()
	{
		return View();
	}

	[HttpPost]
	public async Task<IActionResult> Create(CouponDto coupon)
	{
		if (!ModelState.IsValid)
		{
			return View(coupon);
		}

		var response = await _couponService.CreateCouponAsync(coupon);
		if (response is not {IsSuccess: true})
		{
			TempData["error"] = response?.Message;
			return View(coupon);
		}

		TempData["success"] = "Coupon created successfully";
		return RedirectToAction(nameof(Index));
	}

	[HttpGet]
	public async Task<IActionResult> Delete(int id)
	{
		var response = await _couponService.GetCouponAsync(id);
		if (response?.Result == null || !response.IsSuccess)
		{
			TempData["error"] = response?.Message;
			return RedirectToAction(nameof(Index));
		}

		var resultStr = Convert.ToString(response.Result);
		if (resultStr == null)
		{
			return BadRequest();
		}

		var coupon = JsonConvert.DeserializeObject<CouponDto>(resultStr);

		return View(coupon);
	}

	[HttpPost]
	public async Task<IActionResult> Delete(CouponDto coupon)
	{
		var response = await _couponService.DeleteCouponAsync(coupon.CouponId);
		if (response is not {IsSuccess: true})
		{
			TempData["error"] = response?.Message;
			return RedirectToAction(nameof(Index));
		}

		TempData["success"] = "Coupon deleted successfully";
		return RedirectToAction(nameof(Index));
	}
}
