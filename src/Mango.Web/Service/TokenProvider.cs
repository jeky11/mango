using Mango.Web.Service.IService;

namespace Mango.Web.Service;

public class TokenProvider(IHttpContextAccessor contextAccessor) : ITokenProvider
{
	private readonly IHttpContextAccessor _contextAccessor = contextAccessor;

	public void SetToken(string token)
	{
		_contextAccessor.HttpContext?.Response.Cookies.Append(Constants.TokenCookie, token);
	}

	public string? GetToken()
	{
		string? token = null;
		var hasToken = _contextAccessor.HttpContext?.Request.Cookies.TryGetValue(Constants.TokenCookie, out token);
		return hasToken.HasValue ? token : null;
	}

	public void ClearToken()
	{
		_contextAccessor.HttpContext?.Response.Cookies.Delete(Constants.TokenCookie);
	}
}
