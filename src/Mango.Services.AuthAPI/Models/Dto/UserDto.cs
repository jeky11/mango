namespace Mango.Services.AuthAPI.Models.Dto;

public record UserDto
{
	public required string Id { get; set; }
	public required string Email { get; set; }
	public required string Name { get; set; }
	public required string PhoneNumber { get; set; }
}
