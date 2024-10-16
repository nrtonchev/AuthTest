using Core.Interfaces;
using Infrastructure;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
	public static class ServicesConfiguration
	{
		public static void RegisterServices(this IServiceCollection services)
		{
			services.AddScoped<IMailService, MailService>();
			services.AddScoped<IAuthService, AuthService>();
			services.AddScoped<IAccountService, AccountService>();
			services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
		}

		public static void RegisterCors(this IServiceCollection services)
		{
			services.AddCors(opts =>
			{
				opts.AddPolicy("EnableCors", builder =>
				{
					builder.SetIsOriginAllowed(origin => true)
							.AllowAnyHeader()
							.AllowAnyOrigin()
							.AllowAnyMethod();
				});
			});
		}

		public static void ConfigureDbContext(this IServiceCollection services, IConfiguration config)
		{
			services.AddDbContext<ApplicationContext>(opts =>
			{
				opts.UseNpgsql(config.GetConnectionString("DefaultConnection"));
			});
		}
	}
}
