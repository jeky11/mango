using Mango.Web.Models;
using Mango.Web.Models.Extensions;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;

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
		if (!response.TryGetResult<List<CouponDto>>(out var coupons))
		{
			TempData["error"] = response?.Message ?? "Invalid coupons";
			return View(new List<CouponDto>());
		}

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
		if (!response.TryGetResult<CouponDto>(out var coupon))
		{
			TempData["error"] = response?.Message ?? "Invalid coupon";
			return RedirectToAction(nameof(Index));
		}

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
