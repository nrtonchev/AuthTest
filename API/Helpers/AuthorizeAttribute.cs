using Core.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class AuthorizeAttribute : Attribute, IAuthorizationFilter
	{
		private readonly IEnumerable<Role> roles;

        public AuthorizeAttribute(params Role[] roles)
        {
            this.roles = roles ?? Array.Empty<Role>();
        }

        public void OnAuthorization(AuthorizationFilterContext context)
		{
			var allowAnonymous = context.ActionDescriptor.EndpointMetadata
				.OfType<AllowAnonymousAttribute>()
				.Any();

			if (allowAnonymous)
			{
				return;
			}

			var account = (Account)context.HttpContext.Items["Account"];
			if (account == null || (roles.Any() && !roles.Contains(account.Role)))
			{
				context.Result = new JsonResult(new { message = "Unauthorized " }) { StatusCode = StatusCodes.Status401Unauthorized };
			}
		}
	}
}
