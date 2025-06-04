using Mango.Services.RewardAPI.Data;
using Mango.Services.RewardAPI.Message;
using Mango.Services.RewardAPI.Models;

namespace Mango.Services.RewardAPI.Services;

public class RewardService(AppDbContext context, ILogger<RewardService> logger) : IRewardService
{
	private readonly AppDbContext _context = context;
	private readonly ILogger<RewardService> _logger = logger;

	public async Task UpdateRewardsAsync(RewardsMessage rewardsMessage)
	{
		try
		{
			var rewards = new Rewards
			{
				OrderId = rewardsMessage.OrderId,
				RewardsActivity = rewardsMessage.RewardsActivity,
				UserId = rewardsMessage.UserId,
				RewardsDate = DateTime.Now,
			};

			await _context.Rewards.AddAsync(rewards);
			await _context.SaveChangesAsync();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating rewards");
		}
	}
}
