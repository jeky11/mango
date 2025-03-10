using Mango.Web.Models;

namespace Mango.Web.Service.IService;

public interface IAuthService
{
	Task<ResponseDto?> RegisterAsync(RegistrationRequestDto request);
	Task<ResponseDto?> AssignRoleAsync(AssignRoleRequestDto request);
	Task<ResponseDto?> LoginAsync(LoginRequestDto request);
}
