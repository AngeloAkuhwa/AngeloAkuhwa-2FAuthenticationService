using NumberVerify.Core.Entities;

namespace NumberVerify2FA.Services.Contract
{
		public interface IJwtTokenService
		{
			string GenerateJwtToken(AppUser user);
		}
}
