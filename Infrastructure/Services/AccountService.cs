using AutoMapper;
using Core.Entities.Models;
using Core.Interfaces;
using Core.Requests;
using Core.Responses;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
	public class AccountService : IAccountService
	{
		private readonly ApplicationContext context;
		private readonly IMapper mapper;
		private readonly IMailService mailService;
		private readonly IAuthService authService;

		public AccountService(ApplicationContext context, IMapper mapper, IMailService mailService, IAuthService authService)
        {
			this.context = context;
			this.mapper = mapper;
			this.mailService = mailService;
			this.authService = authService;
		}

        public Task<AccountResponse> Create(CreateRequest model)
		{
			throw new NotImplementedException();
		}

		public Task Delete(int id)
		{
			throw new NotImplementedException();
		}

		public Task ForgotPassword(ForgotPasswordRequest request, string origin)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<AccountResponse>> GetAccounts()
		{
			throw new NotImplementedException();
		}

		public Task<AccountResponse> GetById(int id)
		{
			throw new NotImplementedException();
		}

		public async Task Register(RegisterRequest request, string origin)
		{
			var existing = await context.Accounts.AnyAsync(x => x.Email == request.Email);
			if (existing)
			{
				SendAlreadyRegisteredEmail(request.Email, origin);
				return;
			}

			var account = mapper.Map<Account>(request);

			var isFirstAccount = await context.Accounts.CountAsync() == 0;

			// Set data on register
			account.Role = isFirstAccount ? Role.Admin : Role.User;
			account.Created = DateTime.UtcNow;
			account.VerificationToken = await authService.GenerateVerificationToken();

			account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

			await context.Accounts.AddAsync(account);
			await context.SaveChangesAsync();

			SendVerificationEmail(account, origin);
		}

		public Task ResetPassword(ResetPasswordRequest request)
		{
			throw new NotImplementedException();
		}

		public Task<AccountResponse> Update(int id, UpdateRequest model)
		{
			throw new NotImplementedException();
		}

		#region Private members
		private void SendAlreadyRegisteredEmail(string email, string origin)
		{
			string message;
			if (!string.IsNullOrEmpty(origin))
			{
				message = $@"<p>If you don't know your password please visit the <a href=""{origin}/account/forgot-password"">forgot password</a> page.</p>";
			}
			else 
			{
				message = "<p>If you don't know your password you can reset it via the <code>/accounts/forgot-password</code> api route.</p>";
			}

			var subject = "Sign-up Verification API - Email is already registered";
			var html = $@"<h4>Email Already Registered</h4>
                        <p>Your email <strong>{email}</strong> is already registered.</p>
                        {message}";

			mailService.Send(email, subject, html);
		}

		private void SendVerificationEmail(Account account, string origin)
		{
			string message;

			if (!string.IsNullOrEmpty(origin))
			{
				var verifyUrl = $"{origin}/account/verify-email?token={account.VerificationToken}";
				message = $@"<p>Please click the below link to verify your email address:</p>
                            <p><a href=""{verifyUrl}"">{verifyUrl}</a></p>";
			}
			else
			{
				message = $@"<p>Please use the below token to verify your email address with the <code>/accounts/verify-email</code> api route:</p>
                            <p><code>{account.VerificationToken}</code></p>";
			}

			var subject = "Sign-up Verification API - Verify email";
			var html = $@"<h4>Verify Email</h4>
                        <p>Thanks for registering!</p>
                        {message}";

			mailService.Send(account.Email, subject, html);
		}
		#endregion
	}
}
