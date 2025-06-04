using Mango.Services.EmailAPI.Message;
using Mango.Services.EmailAPI.Models.Dto;

namespace Mango.Services.EmailAPI.Services;

public interface IEmailService
{
	Task EmailCartAndLogAsync(CartDto cartDto);
	Task RegisterUserEmailAndLogAsync(string email);
	Task LogOrderPlacedAsync(RewardsMessage rewardsMessage);
}
