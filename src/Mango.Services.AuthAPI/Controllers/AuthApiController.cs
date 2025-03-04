using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.AuthAPI.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthApiController : ControllerBase
{
	[HttpPost("register")]
	public IActionResult Register()
	{
		return Ok();
	}

	[HttpPost("login")]
	public IActionResult Login()
	{
		return Ok();
	}
}
