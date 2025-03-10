using Mango.Services.AuthAPI.Data;
using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Identity;

namespace Mango.Services.AuthAPI.Service;

public class AuthService : IAuthService
{
	private readonly AppDbContext _dbContext;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly RoleManager<IdentityRole> _roleManager;
	private readonly IJwtTokenGenerator _jwtTokenGenerator;

	public AuthService(
		AppDbContext dbContext,
		UserManager<ApplicationUser> userManager,
		RoleManager<IdentityRole> roleManager,
		IJwtTokenGenerator jwtTokenGenerator)
	{
		_dbContext = dbContext;
		_userManager = userManager;
		_roleManager = roleManager;
		_jwtTokenGenerator = jwtTokenGenerator;
	}

	public async Task<string> Register(RegistrationRequestDto registrationRequestDto)
	{
		var user = new ApplicationUser
		{
			Name = registrationRequestDto.Name,
			UserName = registrationRequestDto.Email,
			Email = registrationRequestDto.Email,
			NormalizedEmail = registrationRequestDto.Email.ToUpper(),
			PhoneNumber = registrationRequestDto.PhoneNumber
		};

		try
		{
			var result = await _userManager.CreateAsync(user, registrationRequestDto.Password);
			if (!result.Succeeded)
			{
				return result.Errors.FirstOrDefault()?.Description ?? string.Empty;
			}

			return string.Empty;
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			return "Error Encountered";
		}
	}

	public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
	{
		var user = _dbContext.ApplicationUsers
			.FirstOrDefault(u => u.UserName != null && u.UserName.ToLower() == loginRequestDto.UserName.ToLower());
		if (user == null)
		{
			return new LoginResponseDto(null, string.Empty);
		}

		var isValid = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);
		if (!isValid)
		{
			return new LoginResponseDto(null, string.Empty);
		}

		var token = _jwtTokenGenerator.GenerateToken(user);

		var userDto = new UserDto
		{
			Id = user.Id,
			Name = user.Name,
			Email = user.Email,
			PhoneNumber = user.PhoneNumber
		};

		return new LoginResponseDto(userDto, token);
	}

	public async Task<bool> AssignRole(string email, string roleName)
	{
		var user = _dbContext.ApplicationUsers.FirstOrDefault(u => u.Email != null && u.Email.ToLower() == email.ToLower());
		if (user == null)
		{
			return false;
		}

		if (!await _roleManager.RoleExistsAsync(roleName))
		{
			await _roleManager.CreateAsync(new IdentityRole(roleName));
		}

		await _userManager.AddToRoleAsync(user, roleName);
		return true;
	}
}
