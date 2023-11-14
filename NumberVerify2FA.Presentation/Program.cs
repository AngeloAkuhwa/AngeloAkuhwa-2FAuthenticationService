using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NumberVerify.Core.Data;
using NumberVerify.Core.Entities;
using NumberVerify.Core.Models;
using NumberVerify2FA.Presentation.Extensions;
using System.Text;
using NumberVerify2FA.Services.Contract;
using NumberVerify2FA.Services.Implementation;

var builder = WebApplication.CreateBuilder(args);

var applicationConfigKeysSection = builder.Configuration.GetSection("ApplicationConfigKeys");
builder.Services.Configure<ApplicationConfigKeys>(applicationConfigKeysSection);
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

var applicationConfigKeys = applicationConfigKeysSection.Get<ApplicationConfigKeys>();
builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
{
		options.UseNpgsql(applicationConfigKeys.DefaultConnectionString);
});

builder.Services.AddIdentity<AppUser, IdentityRole>(cfg =>
	{
		var identityOptions = applicationConfigKeys.IdentityOptions;
		cfg.Lockout.MaxFailedAccessAttempts = identityOptions.Lockout.MaxFailedAccessAttempts;
		cfg.User.RequireUniqueEmail = identityOptions.User.RequireUniqueEmail;
		cfg.Password.RequireDigit = identityOptions.Password.RequireDigit;
		cfg.Password.RequiredUniqueChars = identityOptions.Password.RequiredUniqueChars;
		cfg.Password.RequireLowercase = identityOptions.Password.RequireLowercase;
		cfg.Password.RequireNonAlphanumeric = identityOptions.Password.RequireNonAlphanumeric;
		cfg.Password.RequireUppercase = identityOptions.Password.RequireUppercase;

		cfg.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(identityOptions.Lockout.DefaultLockoutTimeSpanInMinutes);
		cfg.Lockout.MaxFailedAccessAttempts = identityOptions.Lockout.MaxFailedAccessAttempts;
		cfg.Lockout.AllowedForNewUsers = identityOptions.Lockout.AllowedForNewUsers;

		cfg.SignIn.RequireConfirmedAccount = identityOptions.SignIn.RequireConfirmedAccount;

		// 2FA settings.
		cfg.SignIn.RequireConfirmedEmail = identityOptions.SignIn.RequireConfirmedEmail;
		cfg.SignIn.RequireConfirmedPhoneNumber = identityOptions.SignIn.RequireConfirmedPhoneNumber;
})
	.AddEntityFrameworkStores<ApplicationDbContext>()
	.AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
		options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
		options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
	.AddJwtBearer(options =>
	{
			options.TokenValidationParameters = new TokenValidationParameters
			{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = applicationConfigKeys.JwtConfig.Issuer,
					ValidAudience = applicationConfigKeys.JwtConfig.Audience,
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(applicationConfigKeys.JwtConfig.Key)),
					ClockSkew = TimeSpan.Zero
			};
	});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.ConfigureSwagger();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

await app.InitializeApplicationDataAsync(applicationConfigKeys.TestUser);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
	app.UseSwagger();
	app.UseSwaggerUI(c =>
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "2FA.Presentation");
		c.RoutePrefix = "swagger";
	});
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();