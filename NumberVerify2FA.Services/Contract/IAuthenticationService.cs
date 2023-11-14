using NumberVerify.Core.Dto;
using NumberVerify.Core.Entities;
using NumberVerify.Core.Models;

namespace NumberVerify2FA.Services.Contract
{
		public interface IAuthenticationService
		{
			Task<ServiceResponse> EnableAuthenticatorAsync(AppUser user,string phoneNumber);
			Task<ServiceResponse> VerifyAuthenticatorCodeAsync(PhoneVerifyAuthenticatorDto model);
			Task<LoginResponseDto<string>> LoginAsync(LoginDto model);
		}
}
