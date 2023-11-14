namespace NumberVerify.Core.Models
{
	public class ApplicationConfigKeys
	{
		public TwoFactorConfig TwoFactorConfig { get; set; }
		public JwtConfig JwtConfig { get; set; }
		public string DefaultConnectionString { get; set; }
		public IdentityOptions IdentityOptions { get; set; }
		public TestAppUserConfig TestUser { get; set; }
		}

	public class IdentityOptions
	{
		public LockoutOptions Lockout { get; set; }
		public UserOptions User { get; set; }
		public PasswordOptions Password { get; set; }
		public SignInOptions SignIn { get; set; }
	}

	public class LockoutOptions
	{
		public int DefaultLockoutTimeSpanInMinutes { get; set; }
		public int MaxFailedAccessAttempts { get; set; }
		public bool AllowedForNewUsers { get; set; }
	}

	public class UserOptions
	{
		public bool RequireUniqueEmail { get; set; }
	}

	public class PasswordOptions
	{
		public bool RequireDigit { get; set; }
		public int RequiredUniqueChars { get; set; }
		public bool RequireLowercase { get; set; }
		public bool RequireNonAlphanumeric { get; set; }
		public bool RequireUppercase { get; set; }
	}

	public class SignInOptions
	{
		public bool RequireConfirmedAccount { get; set; }
		public bool RequireConfirmedEmail { get; set; }
		public bool RequireConfirmedPhoneNumber { get; set; }
	}

	public class TestAppUserConfig
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string UserName { get; set; }
		public string PhoneNumber { get; set; }
		public string Email { get; set; }
		public string CountryPhoneCode { get; set; }
		public bool EmailConfirmed { get; set; }
		public string Password { get; set; }
	}

	public class JwtConfig
	{
		public string Key { get; set; }
		public string Issuer { get; set; }
		public string Audience { get; set; }
		public string Duration { get; set; }
	}

	public class TwoFactorConfig
	{
		public int CodeValidityDuration { get; set; }
		public int MaxAttempts { get; set; }
	}
}
