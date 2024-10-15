using Core.Entities.Auth;
using Core.Interfaces;
using Infrastructure;
using Microsoft.Extensions.Options;

namespace API.Middlewares
{
	public class JwtTokenMiddleware
	{
		private readonly RequestDelegate next;
		private readonly IOptions<AppSettings> appSettings;

		public JwtTokenMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings)
        {
			this.next = next;
			this.appSettings = appSettings;
		}

		public async Task Invoke(HttpContext context, ApplicationContext applicationContext, IJwtTokenUtils utils)
		{
			var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
			var accountId = utils.ValidateToken(token);

			if (accountId != null)
			{
				// Attach account to context on successful validation
				context.Items["Account"] = await applicationContext.Accounts.FindAsync(accountId.Value);
			}

			await this.next(context);
		}
    }
}
