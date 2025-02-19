using System.Text;
using Mango.Web.Models;
using Mango.Web.Service.IService;
using Newtonsoft.Json;

namespace Mango.Web.Service;

public class BaseService : IBaseService
{
	private readonly IHttpClientFactory _httpClientFactory;

	public BaseService(IHttpClientFactory httpClientFactory)
	{
		_httpClientFactory = httpClientFactory;
	}

	public async Task<ResponseDto?> SendAsync(RequestDto requestDto)
	{
		try
		{
			var client = _httpClientFactory.CreateClient("MangoApi");
			var message = new HttpRequestMessage();
			message.Headers.Add("Accept", "application/json");

			message.RequestUri = new Uri(requestDto.Url);
			if (requestDto.Data != null)
			{
				message.Content = new StringContent(JsonConvert.SerializeObject(requestDto.Data), Encoding.UTF8, "application/json");
			}

			message.Method = new HttpMethod(requestDto.ApiType.ToString());

			var apiResponse = await client.SendAsync(message);
			if (!apiResponse.IsSuccessStatusCode)
			{
				return new ResponseDto
				{
					IsSuccess = false,
					Message = apiResponse.ReasonPhrase ?? apiResponse.StatusCode.ToString()
				};
			}

			var apiContent = await apiResponse.Content.ReadAsStringAsync();
			var apiResponseDto = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
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
