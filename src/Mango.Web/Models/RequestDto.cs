namespace Mango.Web.Models;

public class RequestDto
{
	public HttpMethod ApiType { get; set; } = HttpMethod.Get;
	public required string Url { get; set; }
	public object? Data { get; set; }
	public string? AccessToken { get; set; }

	public MangoMediaType MediaType { get; set; } = MangoMediaType.ApplicationJson;
}
