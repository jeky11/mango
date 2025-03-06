namespace Mango.Services.AuthAPI.Models.Dto;

public record UserDto
{
	public string? Id { get; set; }
	public string? Email { get; set; }
	public string? Name { get; set; }
	public string? PhoneNumber { get; set; }
}
