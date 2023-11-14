using System.ComponentModel.DataAnnotations;

namespace NumberVerify.Core.Entities
{
	public class AppUser : CustomApplicationUser
	{
		[Required] public string FirstName { get; set; }
		[Required] public string LastName { get; set; }
		[Required] public string CountryPhoneCode { get; set; }
		[Required] public DateTime Created { get; set; }
		[Required] public DateTime Updated { get; set; }
		[Required] public int Role { get; set; }
	}
}