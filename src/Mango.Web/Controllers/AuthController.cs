using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace Mango.Web.Controllers;

public class AuthController : Controller
{
	private readonly IAuthService _authService;
	private readonly ITokenProvider _tokenProvider;

	public AuthController(IAuthService authService, ITokenProvider tokenProvider)
	{
		_authService = authService;
		_tokenProvider = tokenProvider;
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
		if (string.IsNullOrEmpty(loginResult?.Token))
		{
			ModelState.AddModelError("CustomError", "Invalid username or password");
			return View(request);
		}

		await SingInUserAsync(loginResult.Token);
		_tokenProvider.SetToken(loginResult.Token);
		TempData["success"] = "Login Successful";
		return RedirectToAction("Index", "Home");
	}

	[HttpGet]
	public async Task<IActionResult> Logout()
	{
		await HttpContext.SignOutAsync();
		_tokenProvider.ClearToken();
		return RedirectToAction("Index", "Home");
	}

	private async Task SingInUserAsync(string token)
	{
		var handler = new JwtSecurityTokenHandler();
		var jwt = handler.ReadJwtToken(token);

		var identity = new ClaimsIdentity(jwt.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
		identity.AddClaim(new Claim(ClaimTypes.Name, jwt.Claims.First(x => x.Type == JwtRegisteredClaimNames.Email).Value));

		var principal = new ClaimsPrincipal(identity);
		await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
	}
}
