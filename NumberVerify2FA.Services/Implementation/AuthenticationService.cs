using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NumberVerify.Core.Dto;
using NumberVerify.Core.Entities;
using NumberVerify.Core.Models;
using NumberVerify2FA.Services.Contract;
using System.Net;

namespace NumberVerify2FA.Services.Implementation
{
		public class AuthenticationService : IAuthenticationService
		{
			private readonly UserManager<AppUser> _userManager;
			private readonly SignInManager<AppUser> _signInManager;
			private readonly ILogger<AuthenticationService> _logger;
			private readonly ApplicationConfigKeys _appConfigKeys;
			private readonly IJwtTokenService _jwtTokenService;
			public AuthenticationService(UserManager<AppUser> userManager, ILogger<AuthenticationService> logger, IOptions<ApplicationConfigKeys> appConfigKeys, SignInManager<AppUser> signInManager, IJwtTokenService jwtTokenService)
			{
					_userManager = userManager;
					_logger = logger;
					_signInManager = signInManager;
					_jwtTokenService = jwtTokenService;
					_appConfigKeys = appConfigKeys.Value;
			}

				public async Task<ServiceResponse> EnableAuthenticatorAsync(AppUser user, string phoneNumber)
				{
						var response = new ServiceResponse();

						if (user.PhoneNumber != phoneNumber)
						{
								response.Message = "Number not found";
								response.StatusCode = (int)HttpStatusCode.BadRequest;
								return response;
						}

						if (user.TwoFactorEnabled || await _userManager.IsPhoneNumberConfirmedAsync(user))
						{
								response.Message = "Two-factor authentication is already enabled.";
								response.StatusCode = (int)HttpStatusCode.BadRequest;
								return response;
						}

						try
						{
								if (await GenerateAndSend2FATokenAsync(phoneNumber))
								{
										response.Message = "Verification code sent via SMS.";
										response.StatusCode = (int)HttpStatusCode.OK;
										return response;
								}

								response.Message = "Failed to send verification code via SMS.";
								response.StatusCode = (int)HttpStatusCode.BadRequest;
								return response;
						}
						catch (Exception ex)
						{
								_logger.LogError(
									"Something went wrong in {EnableAuthenticator} Endpoint; Error: {Error}",
									nameof(EnableAuthenticatorAsync),
									ex);
								response.Message = "Failed to send verification code via SMS.";
								response.StatusCode = (int)HttpStatusCode.InternalServerError;
								return response;
						}
				}

				public async Task<ServiceResponse> VerifyAuthenticatorCodeAsync(PhoneVerifyAuthenticatorDto model)
				{
						var response = new ServiceResponse();
						var appUser = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == model.PhoneNumber);

						if (appUser == null)
						{
								response.Message = "User with the specified phone number not found.";
								response.StatusCode = (int)HttpStatusCode.BadRequest;
								return response;
						}

						if (appUser.TwoFactorEnabled)
						{
								response.Message = "Two-factor authentication is already enabled for this user.";
								response.StatusCode = (int)HttpStatusCode.BadRequest;
								return response;
						}

						if (IsInValid2FACode(model.Token, appUser))
						{
								response.Message = "2FA code has expired.";
								response.StatusCode = (int)HttpStatusCode.BadRequest;
								return response;
						}

						if (appUser.Failed2FAAttempts >= _appConfigKeys.TwoFactorConfig.MaxAttempts)
						{
								appUser.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(_appConfigKeys.IdentityOptions.Lockout.DefaultLockoutTimeSpanInMinutes);
								await _userManager.UpdateAsync(appUser);
								response.Message = "Your account has been locked due to multiple failed attempts.";
								response.StatusCode = (int)HttpStatusCode.BadRequest;
								return response;
						}

						var is2FaTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
								appUser,
								TokenOptions.DefaultPhoneProvider,
								model.Token);

						if (!is2FaTokenValid)
						{
								appUser.Failed2FAAttempts++;
								await _userManager.UpdateAsync(appUser);
								response.Message = "Invalid verification code.";
								response.StatusCode = (int)HttpStatusCode.BadRequest;
								return response;
						}

						// Enable 2FA
						appUser.TwoFactorEnabled = true;
						appUser.PhoneNumberConfirmed = true;
						appUser.EmailConfirmed = true;
						appUser.Failed2FAAttempts = 0;

						var result = await _userManager.UpdateAsync(appUser);

						if (!result.Succeeded)
						{
								response.Message = "An error occurred while enabling two-factor authentication.";
								response.StatusCode = (int)HttpStatusCode.InternalServerError;
								return response;
						}

						response.Message = "Two-factor authentication has been enabled.";
						response.StatusCode = (int)HttpStatusCode.OK;
						return response;
				}

				public async Task<LoginResponseDto<string>> LoginAsync(LoginDto model)
				{
						var response = new LoginResponseDto<string>();
						var user = await _userManager.FindByEmailAsync(model.Email);

						if (user == null)
						{
							response.Message = "Invalid credentials.";
							response.Success = false;
							response.StatusCode = (int)HttpStatusCode.Unauthorized;
							return response;
						}

						if (!await _userManager.IsEmailConfirmedAsync(user))
						{
							response.Message = "You need to confirm your email to log in.";
							response.Success = false;
							response.StatusCode = (int)HttpStatusCode.Unauthorized;
							return response;
						}

						if (await _userManager.IsLockedOutAsync(user))
						{
							response.Message = "Your account has been locked. Please try again later.";
							response.Success = false;
							response.StatusCode = (int)HttpStatusCode.Locked;
							return response;
						}

						var inspectPasswordSignIn = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);
						var token = _jwtTokenService.GenerateJwtToken(user);

						if (inspectPasswordSignIn.Succeeded && !string.IsNullOrWhiteSpace(token))
						{
							if (!user.TwoFactorEnabled)
							{
								response.Data = token;
								response.Success = true;
								response.StatusCode = (int)HttpStatusCode.OK;
								return response;
							}

							//info: this is basically to simulate the 2FA authenticator app integration behaviour for Demo purpose
							var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

								//SignInManager relies on session or cookie data to track the 2FA process
								//for the first time the 2FA user wouldn't be in this flow if already
								//enabled and an session/cookies are no longer available for the user in the current context
								//as a result  await Login2faAsync() would fail	possibly
							if (signInResult.RequiresTwoFactor && await Login2faAsync())
							{
								response.Data = token;
								response.Success = true;
								response.StatusCode = (int)HttpStatusCode.OK;
								return response;
							}

							if (await GenerateAndSend2FATokenAsync(user.PhoneNumber))
							{
								response.Data = token;
								response.Message = "Requires 2FA";
								response.StatusCode = (int)HttpStatusCode.Unauthorized;
								return response;
							}

							response.StatusCode = (int)HttpStatusCode.InternalServerError;
							return response;
						}

						if (inspectPasswordSignIn.IsLockedOut)
						{
							response.Message = "This account has been locked out, please try again later.";
							response.StatusCode = (int)HttpStatusCode.Locked;
							return response;
						}

						if (inspectPasswordSignIn.IsNotAllowed)
						{
							response.Message = "Not allowed to login.";
							response.StatusCode = (int)HttpStatusCode.Unauthorized;
							return response;
						}

						response.Message = "Invalid login attempt.";
						response.StatusCode = (int)HttpStatusCode.Unauthorized;
						return response;
				}

				private async Task<bool> GenerateAndSend2FATokenAsync(string phoneNumber)
				{
					var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

					if (user != null)
					{
						var token = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider);

						user.CodeSentTimestamp = DateTime.UtcNow;
						await _userManager.UpdateAsync(user);

						//TODO: Email/Sms sync here when necessary
						_logger.LogInformation("Your phone verification code is : {Token}", token);
					}

					return user != null;
				}

				private bool IsInValid2FACode(string authenticatorCode, AppUser user)
				{
					return (DateTime.UtcNow - user.CodeSentTimestamp >
										TimeSpan.FromSeconds(_appConfigKeys.TwoFactorConfig.CodeValidityDuration) &&
										_appConfigKeys.TwoFactorConfig.CodeValidityDuration > 0)
										|| authenticatorCode.Length != 6
										|| authenticatorCode.Any(x => !char.IsDigit(x))
										|| string.IsNullOrWhiteSpace(authenticatorCode);
				}

				private async Task<bool> Login2faAsync()
				{
					// Ensure the user has gone through the username & password screen first
					var twoFAUser = await _signInManager.GetTwoFactorAuthenticationUserAsync();
					if (twoFAUser == null)
					{
						return false;
					}

					return await GenerateAndSend2FATokenAsync(twoFAUser.PhoneNumber);
				}
		}
}