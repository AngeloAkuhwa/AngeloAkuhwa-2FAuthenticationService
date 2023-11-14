using Microsoft.AspNetCore.Identity;
using MockQueryable.Moq;
using Moq;
using NumberVerify.Core.Dto;
using NumberVerify.Core.Entities;
using System.Net;
using Xunit;

namespace NumberVerify2FA.Test
{
		public class VerifyAuthenticatorCodeFact : AuthenticationServiceFact
		{
				[Fact]
				public async Task VerifyAuthenticatorCodeAsync_UserNotFound_ReturnsBadRequest()
				{
						// Arrange
						var model = new PhoneVerifyAuthenticatorDto { PhoneNumber = "123456789", Token = "123456" };
						var user = new AppUser { PhoneNumber = "8137778295", TwoFactorEnabled = true };
						MockUserManager.Setup(x => x.Users).Returns(new List<AppUser> { user }.AsQueryable().BuildMock());

						// Act
						var result = await AuthenticationService.VerifyAuthenticatorCodeAsync(model);

						// Assert
						Assert.Equal("User with the specified phone number not found.", result.Message);
						Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
				}

				[Fact]
				public async Task VerifyAuthenticatorCodeAsync_TwoFactorAlreadyEnabled_ReturnsBadRequest()
				{
						// Arrange
						var model = new PhoneVerifyAuthenticatorDto { PhoneNumber = "8137778295", Token = "123456" };
						var user = new AppUser { PhoneNumber = "8137778295", TwoFactorEnabled = true };
						MockUserManager.Setup(x => x.Users).Returns(new List<AppUser> { user }.AsQueryable().BuildMock());

						// Act
						var result = await AuthenticationService.VerifyAuthenticatorCodeAsync(model);

						// Assert
						Assert.Equal("Two-factor authentication is already enabled for this user.", result.Message);
						Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
				}

				[Fact]
				public async Task VerifyAuthenticatorCodeAsync_Invalid2FACode_ReturnsBadRequest()
				{
						// Arrange
						var model = new PhoneVerifyAuthenticatorDto { PhoneNumber = "8137778295", Token = "1234Angelo" };
						var user = new AppUser { PhoneNumber = "8137778295", TwoFactorEnabled = false, Failed2FAAttempts = 0, CodeSentTimestamp = DateTime.UtcNow };
						MockUserManager.Setup(x => x.Users).Returns(new List<AppUser> { user }.AsQueryable().BuildMock);

						// Act
						var result = await AuthenticationService.VerifyAuthenticatorCodeAsync(model);

						// Assert
						Assert.Equal("2FA code has expired.", result.Message);
						Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
				}

				[Fact]
				public async Task VerifyAuthenticatorCodeAsync_UpdateError_ReturnsBadRequest()
				{
						// Arrange
						var model = new PhoneVerifyAuthenticatorDto { PhoneNumber = "8137778295", Token = "123456" };
						var user = new AppUser { PhoneNumber = "8137778295", TwoFactorEnabled = false, Failed2FAAttempts = 0, CodeSentTimestamp = DateTime.UtcNow.AddHours(2) };
						MockUserManager.Setup(x => x.Users).Returns(new List<AppUser> { user }.AsQueryable().BuildMock());
						MockUserManager.Setup(x => x.VerifyTwoFactorTokenAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>()))
							.ReturnsAsync(false);
						MockUserManager.Setup(x => x.UpdateAsync(It.IsAny<AppUser>()))
							.ReturnsAsync(IdentityResult.Failed());

						// Act
						var result = await AuthenticationService.VerifyAuthenticatorCodeAsync(model);

						// Assert
						Assert.Equal("Invalid verification code.", result.Message);
						Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
				}

				[Fact]
				public async Task VerifyAuthenticatorCodeAsync_UpdateError_ReturnsInternalServerError()
				{
						// Arrange
						var model = new PhoneVerifyAuthenticatorDto { PhoneNumber = "8137778295", Token = "123456" };
						var user = new AppUser { PhoneNumber = "8137778295", TwoFactorEnabled = false, Failed2FAAttempts = 0, CodeSentTimestamp = DateTime.UtcNow.AddHours(2) };
						MockUserManager.Setup(x => x.Users).Returns(new List<AppUser> { user }.AsQueryable().BuildMock());
						MockUserManager.Setup(x => x.VerifyTwoFactorTokenAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>()))
							.ReturnsAsync(true);
						MockUserManager.Setup(x => x.UpdateAsync(It.IsAny<AppUser>()))
							.ReturnsAsync(IdentityResult.Failed());

						// Act
						var result = await AuthenticationService.VerifyAuthenticatorCodeAsync(model);

						// Assert
						Assert.Equal("An error occurred while enabling two-factor authentication.", result.Message);
						Assert.Equal((int)HttpStatusCode.InternalServerError, result.StatusCode);
				}

				[Fact]
				public async Task VerifyAuthenticatorCodeAsync_Success_ReturnsOk()
				{
						// Arrange
						var model = new PhoneVerifyAuthenticatorDto { PhoneNumber = "8137778295", Token = "123456" };
						var user = new AppUser { PhoneNumber = "8137778295", TwoFactorEnabled = false, Failed2FAAttempts = 0, CodeSentTimestamp = DateTime.UtcNow.AddHours(2) };
						MockUserManager.Setup(x => x.Users).Returns(new List<AppUser> { user }.AsQueryable().BuildMock);
						MockUserManager.Setup(x => x.VerifyTwoFactorTokenAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>()))
							.ReturnsAsync(true);
						MockUserManager.Setup(x => x.UpdateAsync(It.IsAny<AppUser>()))
							.ReturnsAsync(IdentityResult.Success);

						// Act
						var result = await AuthenticationService.VerifyAuthenticatorCodeAsync(model);

						// Assert
						Assert.Equal("Two-factor authentication has been enabled.", result.Message);
						Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
				}
		}
}
