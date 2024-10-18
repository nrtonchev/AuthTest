using API.Helpers;
using Core.Entities.Models;
using Core.Interfaces;
using Core.Requests;
using Core.Responses;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
    [Authorize]
	public class AccountsController : BaseController
	{
		private readonly IAccountService accountService;

		public AccountsController(IAccountService accountService)
		{
			this.accountService = accountService;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterRequest request)
		{
			await accountService.Register(request, Origin);
			return Ok(
				new { message = "Registration successful, please check your email for verification instructions" });
		}

		[HttpPost("forgot-password")]
		public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
		{
			await accountService.ForgotPassword(request, Origin);
			return Ok(
				new { messge = "Password reset successful, please check your email for verification instructions" });
		}

		[HttpPost("reset-password")]
		public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
		{
			await accountService.ResetPassword(request);
			return Ok(new { message = "Password reset successfully. You can now log in!" });
		}

		[Authorize(Role.Admin)]
		[HttpGet]
		public async Task<ActionResult<IEnumerable<AccountResponse>>> GetAccounts()
		{
			var accounts = await accountService.GetAccounts();
			return Ok(accounts);
		}

		[Authorize]
		[HttpGet("{id:int}")]
		public async Task<ActionResult<AccountResponse>> GetAccount(int id)
		{
			if (id != Account.Id && Account.Role != Role.Admin)
			{
				return Unauthorized( new { message = "Unauthorized!" });
			}

			var account = await accountService.GetById(id);
			return Ok(account);
		}

		[Authorize(Role.Admin)]
		[HttpPost]
		public async Task<ActionResult<AccountResponse>> Create([FromBody] CreateRequest request)
		{
			var account = await accountService.Create(request);
			return Ok(account);
		}

		[Authorize]
		[HttpPut("{id:int}")]
		public async Task<ActionResult<AccountResponse>> Update(int id, [FromBody] UpdateRequest request)
		{
			if (id != Account.Id && Account.Role != Role.Admin)
			{
				return Unauthorized( new { message = "Unauthorized!" });
			}

			if (Account.Role != Role.Admin)
			{
				request.Role = null;
			}
			
			var account = await accountService.Update(id, request);
			return Ok(account);
		}

		[Authorize]
		[HttpDelete("{id:int}")]
		public async Task<IActionResult> Delete(int id)
		{
			if (id != Account.Id && Account.Role != Role.Admin)
			{
				return Unauthorized( new { message = "Unauthorized!" });
			}
			
			await accountService.Delete(id);
			return Ok(new { message = "Account deleted successfully!" });
		}
    }
}
