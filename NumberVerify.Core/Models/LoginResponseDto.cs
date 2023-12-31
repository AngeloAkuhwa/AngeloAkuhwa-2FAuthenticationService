﻿using Microsoft.AspNetCore.Identity;

namespace NumberVerify.Core.Models
{
		public class LoginResponseDto<T>
		{
			public T Data { get; set; }
			public string Message { get; set; }
			public bool Success { get; set; }
			public int StatusCode { get; set; }
		}
}
