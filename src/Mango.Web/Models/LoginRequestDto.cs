namespace Mango.Web.Models;

public record LoginRequestDto
{
	public string UserName { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
}
