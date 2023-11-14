using Microsoft.AspNetCore.Identity;

namespace NumberVerify.Core.Entities
{
	public class CustomApplicationUser : IdentityUser
	{
		public int Failed2FAAttempts { get; set; }
		public DateTime CodeSentTimestamp { get; set; }
	}
}