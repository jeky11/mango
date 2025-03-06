namespace Mango.Services.AuthAPI.Models.Dto;

public record ResponseDto(object? Result, bool IsSuccess, string Message)
{
	public static ResponseDto CreateSuccessResponse() => new(null, true, string.Empty);

	public static ResponseDto CreateSuccessResponse(object? result) => new(result, true, string.Empty);

	public static ResponseDto CreateErrorResponse(string message) => new(null, false, message);
}
