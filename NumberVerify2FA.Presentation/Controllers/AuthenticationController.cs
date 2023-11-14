using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NumberVerify.Core.Dto;
using NumberVerify.Core.Entities;
using NumberVerify2FA.Services.Contract;
using System.Net;

namespace NumberVerify2FA.Presentation.Controllers
{
		[ApiController]
		[Authorize]
		[Route("api/[controller]")]
		public class AuthenticationController : ControllerBase
		{
				private readonly UserManager<AppUser> _userManager;
				private readonly IAuthenticationService _authenticationService;

				public AuthenticationController(UserManager<AppUser> userManager, IAuthenticationService authenticationService)
				{
						_userManager = userManager;
						_authenticationService = authenticationService;
				}

				[HttpPost("login")]
				[AllowAnonymous]
				public async Task<IActionResult> Login([FromBody] LoginDto model)
				{
						if (!ModelState.IsValid)
						{
								return BadRequest(ModelState);
						}

						var result = await _authenticationService.LoginAsync(model);
						return result.Success ? Ok(result) : 
							result is { StatusCode: (int)HttpStatusCode.Unauthorized, Message: "Requires 2FA" } ? 
								StatusCode(result.StatusCode, result.Data) : 
								StatusCode(result.StatusCode, result.Message);
				}

				[HttpGet("enable-authenticator/{phoneNumber}")]
				public async Task<IActionResult> EnableAuthenticator([FromRoute] string phoneNumber)
				{
					var user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber);
					if (user == null)
					{
						return NotFound("User with the specified phone number not found.");
					}

					var result = await _authenticationService.EnableAuthenticatorAsync(user, phoneNumber);
					return StatusCode(result.StatusCode, result.Message);
				}

				[HttpPost("verify-authenticator")]
				public async Task<IActionResult> VerifyAuthenticator([FromBody] PhoneVerifyAuthenticatorDto model)
				{
						var user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == model.PhoneNumber);
						if (user == null)
						{
								return NotFound("User with the specified phone number not found.");
						}

						var result = await _authenticationService.VerifyAuthenticatorCodeAsync(model);
						return StatusCode(result.StatusCode, result.Message);
				}
		}
}