namespace Mango.GatewaySolution.Models;

public record JwtOptions
{
	public string Secret { get; set; } = string.Empty;
	public string Issuer { get; set; } = string.Empty;
	public string Audience { get; set; } = string.Empty;
}
