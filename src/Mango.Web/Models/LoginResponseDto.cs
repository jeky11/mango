namespace Mango.Web.Models;

public record LoginResponseDto(UserDto? User, string Token);
