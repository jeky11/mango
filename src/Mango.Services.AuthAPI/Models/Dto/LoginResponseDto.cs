namespace Mango.Services.AuthAPI.Models.Dto;

public record LoginResponseDto(UserDto? User, string Token);
