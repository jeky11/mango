using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.AuthAPI.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthApiController : ControllerBase
{
	private readonly IAuthService _authService;

	public AuthApiController(IAuthService authService)
	{
		_authService = authService;
	}

	[HttpPost("register")]
	public async Task<IActionResult> Register([FromBody] RegistrationRequestDto model)
	{
		var errorMessage = await _authService.Register(model);
		if (!string.IsNullOrEmpty(errorMessage))
		{
			var response = ResponseDto.CreateErrorResponse(errorMessage);
			return BadRequest(response);
		}

		return Ok(ResponseDto.CreateSuccessResponse());
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
	{
		var loginResponse = await _authService.Login(model);
		if (loginResponse.User == null)
		{
			var response = ResponseDto.CreateErrorResponse("Username or password is incorrect.");
			return BadRequest(response);
		}

		return Ok(ResponseDto.CreateSuccessResponse(loginResponse));
	}

	[HttpPost("assignRole")]
	public async Task<IActionResult> Login([FromBody] AssignRoleRequestDto model)
	{
		var assignRoleSuccessful = await _authService.AssignRole(model.Email, model.Role.ToUpper());
		if (!assignRoleSuccessful)
		{
			var response = ResponseDto.CreateErrorResponse("Error while assigning role.");
			return BadRequest(response);
		}

		return Ok(ResponseDto.CreateSuccessResponse());
	}
}
