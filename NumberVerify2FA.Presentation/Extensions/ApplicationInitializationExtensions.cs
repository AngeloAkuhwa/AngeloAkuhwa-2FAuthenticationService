using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NumberVerify.Core.Data;
using NumberVerify.Core.Entities;
using NumberVerify.Core.Helpers;
using NumberVerify.Core.Models;

namespace NumberVerify2FA.Presentation.Extensions
{
    public static class ApplicationInitializationExtensions
	{
		public static async Task InitializeApplicationDataAsync(this WebApplication app, TestAppUserConfig testUser)
		{
			await using var scope = app.Services.CreateAsyncScope();
			var appContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
			var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

			await appContext.Database.MigrateAsync();
			await EnsureRolesAsync(roleManager, new[] { nameof(Role.Admin), nameof(Role.GeneralUser) });

			var user = await userManager.FindByEmailAsync("superadmin@ArivalBank.org");
			if (user == null)
			{
				user = new AppUser
				{
					FirstName = testUser.FirstName,
					LastName = testUser.LastName,
					UserName = testUser.UserName,
					PhoneNumber = testUser.PhoneNumber,
					Email = testUser.Email,
					CountryPhoneCode = testUser.CountryPhoneCode,
					EmailConfirmed = testUser.EmailConfirmed,
					Created = DateTime.UtcNow,
					Updated = DateTime.UtcNow,
					Role = (int)Role.Admin
				};

				var createUserResult = await userManager.CreateAsync(user, testUser.Password);
				if (createUserResult != IdentityResult.Success)
				{
					throw new InvalidOperationException("Failed to create default super admin user");
				}

				var addToRoleResult = await userManager.AddToRoleAsync(user, nameof(Role.Admin));
				if (addToRoleResult != IdentityResult.Success)
				{
					throw new InvalidOperationException("Failed to assign super admin role to default user");
				}
			}
		}

		public static void ConfigureSwagger(this WebApplicationBuilder builder)
		{
			builder.Services.AddSwaggerGen(options =>
			{
				options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
				{
					Title = "Your API Title",
					Version = "v1"
				});

				// Define the BearerAuth scheme that's in use
				options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
				{
					Description = "Authorization: Bearer {token}",
					Name = "Authorization",
					In = Microsoft.OpenApi.Models.ParameterLocation.Header,
					Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
					Scheme = "Bearer"
				});

				// Make sure swagger knows to use this security definition
				options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
				{
					{
						new Microsoft.OpenApi.Models.OpenApiSecurityScheme
						{
							Reference = new Microsoft.OpenApi.Models.OpenApiReference
							{
								Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
								Id = "Bearer"
							},
							Scheme = "JWT Auth",
							Name = "Bearer",
							In = Microsoft.OpenApi.Models.ParameterLocation.Header,
						},
						new List<string>()
					}
				});
			});
		}

		private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager, IEnumerable<string> roles)
		{
			foreach (var role in roles)
			{
				if (await roleManager.RoleExistsAsync(role))
				{
					continue;
				}

				var result = await roleManager.CreateAsync(new IdentityRole(role));
				if (!result.Succeeded)
				{
					throw new InvalidOperationException($"Failed to create {role} role");
				}
			}
		}
	}
}
