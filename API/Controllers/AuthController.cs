using API.Helpers;
using Core.Entities.Models;
using Core.Interfaces;
using Core.Requests;
using Core.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : BaseController
	{
		private readonly IAuthService authService;

		public AuthController(IAuthService authService)
		{
			this.authService = authService;
		}

		[HttpPost("authenticate")]
		public async Task<ActionResult<AuthResponse>> Authenticate([FromBody] AuthRequest authRequest)
		{
			var response = await authService.Authenticate(authRequest, GetIpAddress());
			SetTokenCookie(response.RefreshToken);
			return Ok(response);
		}

		[HttpPost("refresh-token")]
		public async Task<ActionResult<AuthResponse>> RefreshToken()
		{
			var refreshToken = Request.Cookies["refreshToken"];
			var response = await authService.RefreshToken(refreshToken, GetIpAddress());
			SetTokenCookie(response.RefreshToken);
			return Ok(response);
		}

		[HttpPost("revoke-token")]
		public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
		{
			var token = request.Token ?? Request.Cookies["refreshToken"];
			if (string.IsNullOrEmpty(token))
			{
				return BadRequest(new { message = "Token is required" });
			}

			if (!Account.OwnsToken(token) && Account.Role != Role.Admin)
			{
				return Unauthorized(new { message = "Unauthorized" });
			}
			
			await authService.RevokeToken(token, GetIpAddress());
			return Ok(new { message = "Token revoked" });
		}
		
		[HttpPost("verify-email")]
		public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
		{
			await authService.VerifyEmail(request.Token);
			return Ok(new { message = "Verification was successful. You can now login to your account!" });
		}

		[HttpPost("validate-reset-token")]
		public async Task<IActionResult> ValidateResetToken([FromBody] ValidateResetTokenRequest request)
		{
			await authService.ValidateResetToken(request);
			return Ok(new { message = "Token is valid" });
		}
		
		#region Private members
		private void SetTokenCookie(string token)
		{
			var cookieOptions = new CookieOptions
			{
				HttpOnly = true,
				Expires = DateTime.UtcNow.AddDays(7)
			};
		
			Response.Cookies.Append("refreshToken", token, cookieOptions);
		}

		private string GetIpAddress()
		{
			if (Request.Headers.ContainsKey("X-Forwarded-For"))
			{
				return Request.Headers["X-Forwarded-For"];
			}
			else
			{
				return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
			}
		}
		#endregion
	}
}
