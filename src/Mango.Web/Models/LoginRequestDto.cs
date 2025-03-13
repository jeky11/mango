using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Models;

public record LoginRequestDto
{
	[Required]
	public string UserName { get; set; } = string.Empty;

	[Required]
	public string Password { get; set; } = string.Empty;
}
