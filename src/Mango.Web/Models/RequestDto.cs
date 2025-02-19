using HttpMethod = Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpMethod;

namespace Mango.Web.Models;

public class RequestDto
{
	public HttpMethod ApiType { get; set; } = HttpMethod.Get;
	public required string Url { get; set; }
	public object? Data { get; set; }
	public string? AccessToken { get; set; }
}
