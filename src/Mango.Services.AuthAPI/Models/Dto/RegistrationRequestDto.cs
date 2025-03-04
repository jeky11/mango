namespace Mango.Services.AuthAPI.Models.Dto;

public record RegistrationRequestDto
{
	public required string Email { get; set; }
	public required string Name { get; set; }
	public required string PhoneNumber { get; set; }
	public required string Password { get; set; }
}
