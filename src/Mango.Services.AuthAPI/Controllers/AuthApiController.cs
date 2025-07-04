using Mango.MessageBus.MessageBusSender;
using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Mango.Services.AuthAPI.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthApiController(IAuthService authService, IMessageBusSender messageBusSender, IOptions<TopicAndQueueNames> topicAndQueueNames) : ControllerBase
{
	private readonly IAuthService _authService = authService;
	private readonly IMessageBusSender _messageBusSender = messageBusSender;
	private readonly TopicAndQueueNames _topicAndQueueNames = topicAndQueueNames.Value;

	[HttpPost("register")]
	public async Task<IActionResult> Register([FromBody] RegistrationRequestDto model)
	{
		var errorMessage = await _authService.Register(model);
		if (!string.IsNullOrEmpty(errorMessage))
		{
			var response = ResponseDto.CreateErrorResponse(errorMessage);
			return BadRequest(response);
		}

		await _messageBusSender.PublishMessageToQueueAsync(model.Email, _topicAndQueueNames.RegisterUserQueue);

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
