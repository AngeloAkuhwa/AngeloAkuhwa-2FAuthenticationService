using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NumberVerify.Core.Entities;
using NumberVerify.Core.Models;
using NumberVerify2FA.Services.Contract;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NumberVerify2FA.Services.Implementation
{
		public class JwtTokenService  : IJwtTokenService
		{
			private readonly ILogger<JwtTokenService> _logger;
			private readonly ApplicationConfigKeys _appConfigKeys;

			public JwtTokenService(ILogger<JwtTokenService> logger, IOptions<ApplicationConfigKeys> appConfigKeys)
			{
				_logger = logger;
				_appConfigKeys = appConfigKeys.Value;
			}

			public string GenerateJwtToken(AppUser user)
			{
				try
				{
					var claims = new List<Claim>
					{
						new(ClaimTypes.Email, user.UserName),
						new(ClaimTypes.MobilePhone, user.PhoneNumber),
						new(ClaimTypes.NameIdentifier, user.Id),
						new(ClaimTypes.Role, user.Role.ToString()),
						new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
					};

					var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appConfigKeys.JwtConfig.Key));
					var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
					var expires = DateTime.UtcNow.AddHours(Convert.ToDouble(_appConfigKeys.JwtConfig.Duration));

					var tokenDescriptor = new SecurityTokenDescriptor
					{
						Subject = new ClaimsIdentity(claims),
						Expires = expires,
						SigningCredentials = credentials,
						Issuer = _appConfigKeys.JwtConfig.Issuer,
						Audience = _appConfigKeys.JwtConfig.Audience
					};

					var tokenHandler = new JwtSecurityTokenHandler();
					var token = tokenHandler.CreateToken(tokenDescriptor);

					return tokenHandler.WriteToken(token);
				}
				catch (Exception e)
				{
					_logger.LogInformation("Authentication token generation failed in : {GenerateJwtToken}", nameof(GenerateJwtToken));
					return string.Empty;
				}
			}
		}
}
