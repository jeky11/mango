using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.Extensions.Options;
using HttpMethod = System.Net.Http.HttpMethod;

namespace Mango.Web.Service;

public class AuthService : IAuthService
{
	private readonly IBaseService _baseService;
	private readonly Uri _baseUrl;

	public AuthService(IBaseService baseService, IOptions<ServiceUrls> serviceUrls)
	{
		_baseService = baseService;
		_baseUrl = new Uri(serviceUrls.Value.AuthApi);
	}

	public async Task<ResponseDto?> RegisterAsync(RegistrationRequestDto request)
	{
		var responseDto = await _baseService.SendAsync(
			new RequestDto
			{
				ApiType = HttpMethod.Post,
				Url = new Uri(_baseUrl, "/api/auth/register").ToString(),
				Data = request
			});
		return responseDto;
	}

	public async Task<ResponseDto?> AssignRoleAsync(AssignRoleRequestDto request)
	{
		var responseDto = await _baseService.SendAsync(
			new RequestDto
			{
				ApiType = HttpMethod.Post,
				Url = new Uri(_baseUrl, "/api/auth/assignRole").ToString(),
				Data = request
			});
		return responseDto;
	}

	public async Task<ResponseDto?> LoginAsync(LoginRequestDto request)
	{
		var responseDto = await _baseService.SendAsync(
			new RequestDto
			{
				ApiType = HttpMethod.Post,
				Url = new Uri(_baseUrl, "/api/auth/login").ToString(),
				Data = request
			});
		return responseDto;
	}
}
