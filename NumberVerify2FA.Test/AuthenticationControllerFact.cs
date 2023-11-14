using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MockQueryable.Moq;
using Moq;
using NumberVerify.Core.Dto;
using NumberVerify.Core.Entities;
using NumberVerify.Core.Models;
using NumberVerify2FA.Presentation.Controllers;
using NumberVerify2FA.Services.Contract;
using System.Net;
using Xunit;

namespace NumberVerify2FA.Test
{
		public class AuthenticationControllerFact
		{
				private readonly Mock<UserManager<AppUser>> _mockUserManager;
				private readonly Mock<IAuthenticationService> _mockAuthenticationService;
				private readonly AuthenticationController _controller;

				public AuthenticationControllerFact()
				{
						_mockUserManager = new Mock<UserManager<AppUser>>(
								Mock.Of<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);

						_mockAuthenticationService = new Mock<IAuthenticationService>();

						_controller = new AuthenticationController(_mockUserManager.Object, _mockAuthenticationService.Object);
				}

				[Fact]
				public async Task EnableAuthenticator_UserNotFound_ReturnsNotFound()
				{
						// Arrange
						string phoneNumber = "123456789";
						_mockUserManager.Setup(x => x.Users).Returns(new List<AppUser>().AsQueryable().BuildMock());

						// Act
						var result = await _controller.EnableAuthenticator(phoneNumber);

						// Assert
						var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
						Assert.Equal("User with the specified phone number not found.", notFoundResult.Value);
				}

				[Fact]
				public async Task EnableAuthenticator_Success_ReturnsCorrectStatusCode()
				{
						// Arrange
						string phoneNumber = "123456789";
						var user = new AppUser { PhoneNumber = phoneNumber };
						_mockUserManager.Setup(x => x.Users).Returns(new List<AppUser> { user }.AsQueryable().BuildMock());
						_mockAuthenticationService.Setup(s => s.EnableAuthenticatorAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
																			.ReturnsAsync(new ServiceResponse { StatusCode = (int)HttpStatusCode.OK, Message = "Success" });

						// Act
						var result = await _controller.EnableAuthenticator(phoneNumber);

						// Assert
						var statusCodeResult = Assert.IsType<ObjectResult>(result);
						Assert.Equal((int)HttpStatusCode.OK, statusCodeResult.StatusCode);
						Assert.Equal("Success", statusCodeResult.Value);
				}

				[Fact]
				public async Task VerifyAuthenticator_UserNotFound_ReturnsNotFound()
				{
					// Arrange
					var model = new PhoneVerifyAuthenticatorDto { PhoneNumber = "123456789", Token = "123456" };
					_mockUserManager.Setup(x => x.Users).Returns(new List<AppUser>().AsQueryable().BuildMock());

					// Act
					var result = await _controller.VerifyAuthenticator(model);

					// Assert
					var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
					Assert.Equal("User with the specified phone number not found.", notFoundResult.Value);
				}

				[Fact]
				public async Task VerifyAuthenticator_Success_ReturnsCorrectStatusCode()
				{
					// Arrange
					var model = new PhoneVerifyAuthenticatorDto { PhoneNumber = "123456789", Token = "123456" };
					var user = new AppUser { PhoneNumber = "123456789" };
					_mockUserManager.Setup(x => x.Users).Returns(new List<AppUser> { user }.AsQueryable().BuildMock());
					_mockAuthenticationService.Setup(s => s.VerifyAuthenticatorCodeAsync(It.IsAny<PhoneVerifyAuthenticatorDto>()))
						.ReturnsAsync(new ServiceResponse { StatusCode = (int)HttpStatusCode.OK, Message = "Success" });

					// Act
					var result = await _controller.VerifyAuthenticator(model);

					// Assert
					var statusCodeResult = Assert.IsType<ObjectResult>(result);
					Assert.Equal((int)HttpStatusCode.OK, statusCodeResult.StatusCode);
					Assert.Equal("Success", statusCodeResult.Value);
				}
		}
}
