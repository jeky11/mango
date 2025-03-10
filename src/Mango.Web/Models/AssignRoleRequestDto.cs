namespace Mango.Web.Models;

public record AssignRoleRequestDto
{
	public required string Email { get; set; }
	public required string Role { get; set; }
}
