using System.Text;
using Mango.Services.EmailAPI.Data;
using Mango.Services.EmailAPI.Message;
using Mango.Services.EmailAPI.Models;
using Mango.Services.EmailAPI.Models.Dto;

namespace Mango.Services.EmailAPI.Services;

public class EmailService(AppDbContext context, ILogger<EmailService> logger) : IEmailService
{
	private readonly AppDbContext _context = context;
	private readonly ILogger<EmailService> _logger = logger;

	public async Task EmailCartAndLogAsync(CartDto cartDto)
	{
		if (cartDto.CartHeader == null || cartDto.CartDetails == null || cartDto.CartHeader.Email == null)
		{
			throw new Exception("CartHeader and cartDetails are required");
		}

		var message = new StringBuilder();

		message.AppendLine("<br/>Cart Email Requested ");
		message.AppendLine("<br/>Total  " + cartDto.CartHeader.CartTotal);
		message.Append("<br/>");
		message.Append("<ul>");
		foreach (var item in cartDto.CartDetails)
		{
			message.Append("<li>");
			message.Append(item.Product?.Name + " x " + item.Count);
			message.Append("</li>");
		}

		message.Append("</ul>");

		await LogAndEmailAsync(message.ToString(), cartDto.CartHeader.Email);
	}

	public async Task RegisterUserEmailAndLogAsync(string email)
	{
		var message = "User Registered. <br/> Email: " + email;
		await LogAndEmailAsync(message, "dotnetmastery@gmail.com");
	}

	public async Task LogOrderPlacedAsync(RewardsMessage rewardsMessage)
	{
		var message = "New Order Placed. <br/> Order Id: " + rewardsMessage.OrderId;
		await LogAndEmailAsync(message, "dotnetmastery@gmail.com");
	}

	private async Task<bool> LogAndEmailAsync(string message, string email)
	{
		try
		{
			var emailLog = new EmailLogger
			{
				Email = email,
				EmailSent = DateTime.Now,
				Message = message
			};

			await _context.EmailLoggers.AddAsync(emailLog);
			await _context.SaveChangesAsync();
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occured while logging email");
			return false;
		}
	}
}
