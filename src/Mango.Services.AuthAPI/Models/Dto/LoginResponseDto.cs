namespace Mango.Services.AuthAPI.Models.Dto;

public record LoginResponseDto
{
	public required UserDto User { get; set; }
	public required string Token { get; set; }
}
