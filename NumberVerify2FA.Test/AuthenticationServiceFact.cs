using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NumberVerify.Core.Entities;
using NumberVerify.Core.Models;
using NumberVerify2FA.Services.Contract;
using NumberVerify2FA.Services.Implementation;

namespace NumberVerify2FA.Test
{
		public class AuthenticationServiceFact
		{
			protected readonly Mock<UserManager<AppUser>> MockUserManager;
			protected readonly Mock<SignInManager<AppUser>> MockSignInManager;
			protected readonly Mock<ILogger<AuthenticationService>> MockLogger;
			protected readonly AuthenticationService AuthenticationService;
			protected readonly Mock<IJwtTokenService> MockJwtTokenService;
			protected ApplicationConfigKeys TestConfigKeys;

			public AuthenticationServiceFact()
			{
				MockUserManager = new Mock<UserManager<AppUser>>(Mock.Of<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);
				MockSignInManager = new Mock<SignInManager<AppUser>>(MockUserManager.Object, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<AppUser>>(), null, null, null, null);
				MockLogger = new Mock<ILogger<AuthenticationService>>();
				MockJwtTokenService = new Mock<IJwtTokenService>();
				TestConfigKeys = new ApplicationConfigKeys()
				{
					TwoFactorConfig = new TwoFactorConfig()
					{
						MaxAttempts = 4,
						CodeValidityDuration = 60
					}
				};

				var appConfigKeys = Options.Create(TestConfigKeys);
				var mockUserStore = new Mock<IUserStore<AppUser>>();
				MockUserManager = new Mock<UserManager<AppUser>>(mockUserStore.Object, null, null, null, null, null, null, null, null);
				AuthenticationService = new AuthenticationService(MockUserManager.Object, MockLogger.Object, appConfigKeys, MockSignInManager.Object, MockJwtTokenService.Object);
			}
			
		}
}
