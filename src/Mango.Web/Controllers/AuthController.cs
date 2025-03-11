using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

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
		ViewBag.RoleList = Enum.GetNames<Role>().Select(x => new SelectListItem(x, x)).ToList();

		var registrationResponse = await _authService.RegisterAsync(request);
		if (registrationResponse is not {IsSuccess: true})
		{
			TempData["error"] = registrationResponse?.Message;
			return View(request);
		}

		var assignRoleResponse = await _authService.AssignRoleAsync(
			new AssignRoleRequestDto
			{
				Email = request.Email,
				Role = request.Role
			});
		if (assignRoleResponse is not {IsSuccess: true})
		{
			TempData["error"] = assignRoleResponse?.Message;
			return View(request);
		}

		TempData["success"] = "Registration Successful";
		return RedirectToAction(nameof(Login));
	}

	[HttpGet]
	public IActionResult Login()
	{
		var loginRequestDto = new LoginRequestDto();
		return View(loginRequestDto);
	}

	[HttpPost]
	public async Task<IActionResult> Login(LoginRequestDto request)
	{
		var loginResponse = await _authService.LoginAsync(request);
		if (loginResponse is not {IsSuccess: true})
		{
			ModelState.AddModelError("CustomError", loginResponse?.Message ?? string.Empty);
			return View(request);
		}

		var loginResult = JsonConvert.DeserializeObject<LoginResponseDto>(Convert.ToString(loginResponse.Result) ?? string.Empty);
		if (loginResult?.User == null)
		{
			ModelState.AddModelError("CustomError", "Invalid username or password");
			return View(request);
		}

		TempData["success"] = "Login Successful";
		return RedirectToAction("Index", "Home");
	}

	[HttpGet]
	public IActionResult Logout()
	{
		return View();
	}
}
