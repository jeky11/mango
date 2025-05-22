using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Mango.Services.Infrastructure;

public class BackendApiAuthenticationHttpClientHandler : DelegatingHandler
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public BackendApiAuthenticationHttpClientHandler(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		var httpContext = _httpContextAccessor.HttpContext;
		if (httpContext != null)
		{
			var token = await httpContext.GetTokenAsync("access_token");
			if (token != null)
			{
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			}
		}

		return await base.SendAsync(request, cancellationToken);
	}
}
