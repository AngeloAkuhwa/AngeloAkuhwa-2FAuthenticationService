using Microsoft.AspNetCore.Identity;
using MockQueryable.Moq;
using Moq;
using NumberVerify.Core.Entities;
using System.Net;
using Xunit;

namespace NumberVerify2FA.Test
{
		public class EnableAuthenticatorAsync	 : AuthenticationServiceFact
		{
			[Fact]
			public async Task EnableAuthenticatorAsync_PhoneNumberMismatch_ReturnsBadRequest()
			{
				// Arrange
				var user = new AppUser { PhoneNumber = "123456789" };

				// Act
				var result = await AuthenticationService.EnableAuthenticatorAsync(user, "987654321");

				// Assert
				Assert.Equal("Number not found", result.Message);
				Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
			}

			[Fact]
			public async Task EnableAuthenticatorAsync_TwofactorAlreadyEnabled_ReturnsBadRequest()
			{
				// Arrange
				var user = new AppUser { PhoneNumber = "123456789", TwoFactorEnabled = true };
				MockUserManager.Setup(x => x.IsPhoneNumberConfirmedAsync(It.IsAny<AppUser>())).ReturnsAsync(true);

				// Act
				var result = await AuthenticationService.EnableAuthenticatorAsync(user, "123456789");

				// Assert
				Assert.Equal("Two-factor authentication is already enabled.", result.Message);
				Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
			}

			[Fact]
			public async Task EnableAuthenticatorAsync_Success_ReturnsOk()
			{
				// Arrange
				var user = new AppUser { PhoneNumber = "123456789" };
				MockUserManager.Setup(x => x.Users).Returns(new List<AppUser> { user }.AsQueryable().BuildMock());
				MockUserManager.Setup(x => x.GenerateTwoFactorTokenAsync(It.IsAny<AppUser>(), TokenOptions.DefaultPhoneProvider)).ReturnsAsync("123456");
				MockUserManager.Setup(x => x.UpdateAsync(It.IsAny<AppUser>())).ReturnsAsync(IdentityResult.Success);
						
				// Act
				var result = await AuthenticationService.EnableAuthenticatorAsync(user, "123456789");

				// Assert
				Assert.Equal("Verification code sent via SMS.", result.Message);
				Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
			}

			[Fact]
			public async Task EnableAuthenticatorAsync_FailedToSendToken_ReturnsBadRequest()
			{
				// Arrange
				var user = new AppUser { PhoneNumber = "123456789" };
				var userTwo = new AppUser { PhoneNumber = "1234239" };
				string phoneNumber = "1234239";
				MockUserManager.Setup(x => x.Users).Returns(new List<AppUser> { user }.AsQueryable().BuildMock());
				MockUserManager.Setup(x => x.GenerateTwoFactorTokenAsync(It.IsAny<AppUser>(), TokenOptions.DefaultPhoneProvider)).ReturnsAsync("123456");

				// Act
				var result = await AuthenticationService.EnableAuthenticatorAsync(userTwo, phoneNumber);

				// Assert
				Assert.Equal("Failed to send verification code via SMS.", result.Message);
				Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
			}

			[Fact]
			public async Task EnableAuthenticatorAsync_ExceptionOccurs_ReturnsInternalServerError()
			{
				// Arrange
				var user = new AppUser { PhoneNumber = "123456789" };
				string phoneNumber = "123456789";
				MockUserManager.Setup(x => x.GenerateTwoFactorTokenAsync(It.IsAny<AppUser>(), TokenOptions.DefaultPhoneProvider)).ThrowsAsync(new Exception("Simulated Exception"));

				// Act
				var result = await AuthenticationService.EnableAuthenticatorAsync(user, phoneNumber);

				// Assert
				Assert.Equal("Failed to send verification code via SMS.", result.Message);
				Assert.Equal((int)HttpStatusCode.InternalServerError, result.StatusCode);
			}
		}
}