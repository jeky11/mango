using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Mango.Web.Models;
using Mango.Web.Models.Extensions;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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
		if (!loginResponse.TryGetResult<LoginResponseDto>(out var loginResult))
		{
			TempData["error"] = loginResponse?.Message ?? "Invalid login";
			return View(request);
		}

		if (string.IsNullOrEmpty(loginResult.Token))
		{
			TempData["error"] = "Invalid username or password";
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

		var claimTypesToExtract = new HashSet<string>
		{
			JwtRegisteredClaimNames.Email,
			JwtRegisteredClaimNames.Sub,
			JwtRegisteredClaimNames.Name,
			"role"
		};

		var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
		identity.AddClaims(jwt.Claims.Where(x => claimTypesToExtract.Contains(x.Type)).Select(x => new Claim(x.Type, x.Value)));
		identity.AddClaim(new Claim(ClaimTypes.Name, jwt.Claims.First(x => x.Type == JwtRegisteredClaimNames.Email).Value));

		var principal = new ClaimsPrincipal(identity);
		await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
	}
}
