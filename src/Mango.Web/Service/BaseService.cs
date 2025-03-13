using System.Text;
using Mango.Web.Models;
using Mango.Web.Service.IService;
using Newtonsoft.Json;

namespace Mango.Web.Service;

public class BaseService : IBaseService
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly ITokenProvider _tokenProvider;

	public BaseService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider)
	{
		_httpClientFactory = httpClientFactory;
		_tokenProvider = tokenProvider;
	}

	public async Task<ResponseDto?> SendAsync(RequestDto requestDto, bool withBearer = true)
	{
		try
		{
			var client = _httpClientFactory.CreateClient("MangoApi");
			var message = new HttpRequestMessage();
			message.Headers.Add("Accept", "application/json");

			if (withBearer)
			{
				var token = _tokenProvider.GetToken();
				message.Headers.Add("Authorization", $"Bearer {token}");
			}

			message.RequestUri = new Uri(requestDto.Url);
			if (requestDto.Data != null)
			{
				message.Content = new StringContent(JsonConvert.SerializeObject(requestDto.Data), Encoding.UTF8, "application/json");
			}

			message.Method = requestDto.ApiType;

			var apiResponse = await client.SendAsync(message);
			var apiContent = await apiResponse.Content.ReadAsStringAsync();
			var apiResponseDto = JsonConvert.DeserializeObject<ResponseDto>(apiContent);

			if (!apiResponse.IsSuccessStatusCode && apiResponseDto == null)
			{
				apiResponseDto = new ResponseDto
				{
					IsSuccess = false,
					Message = apiResponse.ReasonPhrase ?? apiResponse.StatusCode.ToString()
				};
			}

			return apiResponseDto;
		}
		catch (Exception e)
		{
			return new ResponseDto
			{
				IsSuccess = false,
				Message = e.Message
			};
		}
	}
}
