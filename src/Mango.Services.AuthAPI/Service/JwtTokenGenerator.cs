using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Mango.Services.AuthAPI.Service;

public class JwtTokenGenerator(IOptions<JwtOptions> jwtOptions) : IJwtTokenGenerator
{
	private readonly JwtOptions _jwtOptions = jwtOptions.Value;

	public string GenerateToken(ApplicationUser applicationUser, IEnumerable<string> roles)
	{
		var key = Encoding.ASCII.GetBytes(_jwtOptions.Secret);

		var claims = new List<Claim>
		{
			new(JwtRegisteredClaimNames.Email, applicationUser.Email ?? string.Empty),
			new(JwtRegisteredClaimNames.Sub, applicationUser.Id),
			new(JwtRegisteredClaimNames.Name, applicationUser.UserName ?? string.Empty),
		};

		claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Audience = _jwtOptions.Audience,
			Issuer = _jwtOptions.Issuer,
			Subject = new ClaimsIdentity(claims),
			Expires = DateTime.UtcNow.AddDays(7),
			SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
		};

		var tokenHandler = new JwtSecurityTokenHandler();
		var token = tokenHandler.CreateToken(tokenDescriptor);

		return tokenHandler.WriteToken(token);
	}
}
