using System.ComponentModel.DataAnnotations;

namespace NumberVerify.Core.Dto
{
		public class LoginDto
		{
				[Required]
				public string Email { get; set; }
				[Required]
				public string Password { get; set; }
				public string PhoneNumber { get; set; }
		}
}
