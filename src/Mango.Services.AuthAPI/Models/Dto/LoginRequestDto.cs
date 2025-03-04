namespace Mango.Services.AuthAPI.Models.Dto;

public record LoginRequestDto
{
	public required string UserName { get; set; }
	public required string Password { get; set; }
}
