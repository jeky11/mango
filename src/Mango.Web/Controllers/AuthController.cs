using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;

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
		return View();
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
