using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Mango.Web.Models.Extensions;

public static class ResponseDtoExtensions
{
	public static bool TryGetResult<T>(this ResponseDto? responseDto, [NotNullWhen(true)] out T? result)
	{
		if (responseDto?.Result == null || !responseDto.IsSuccess)
		{
			result = default;
			return false;
		}

		var responseStr = Convert.ToString(responseDto.Result);
		if (responseStr == null)
		{
			result = default;
			return false;
		}

		result = JsonConvert.DeserializeObject<T>(responseStr);
		if (result == null)
		{
			return false;
		}

		return true;
	}
}
