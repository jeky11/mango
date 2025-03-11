using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Mango.Web.Controllers;

public class AuthController : Controller
{
	private readonly IAuthService _authService;

	public AuthController(IAuthService authService)
	{
		_authService = authService;
	}

	[HttpGet]
	public IActionResult Register()
	{
		ViewBag.RoleList = Enum.GetNames<Role>().Select(x => new SelectListItem(x, x)).ToList();
		return View();
	}

	[HttpPost]
	public async Task<IActionResult> Register(RegistrationRequestDto request)
	{
		var registrationResult = await _authService.RegisterAsync(request);
		if (registrationResult is not {IsSuccess: true})
		{
			TempData["error"] = registrationResult?.Message;
			return RedirectToAction(nameof(Register));
		}

		var assignRoleResult = await _authService.AssignRoleAsync(
			new AssignRoleRequestDto
			{
				Email = request.Email,
				Role = request.Role
			});
		if (assignRoleResult is not {IsSuccess: true})
		{
			TempData["error"] = assignRoleResult?.Message;
			return RedirectToAction(nameof(Register));
		}

		TempData["success"] = "Registration Successful";
		return RedirectToAction(nameof(Login));
	}

	[HttpGet]
	public IActionResult Login()
	{
		var loginRequestDto = new LoginRequestDto
		{
			UserName = "",
			Password = ""
		};
		return View(loginRequestDto);
	}

	[HttpGet]
	public IActionResult Logout()
	{
		return View();
	}
}
