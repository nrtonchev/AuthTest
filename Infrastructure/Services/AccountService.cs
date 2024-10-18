using AutoMapper;
using Core;
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

        public async Task<AccountResponse> Create(CreateRequest model)
		{
			var existing = await context.Accounts.AnyAsync(x => x.Email == model.Email);

			if (!existing) 
			{
				throw new DomainException($"An account with email: '{model.Email}', is already registered!");
			}

			var account = mapper.Map<Account>(model);
			account.Created = DateTime.UtcNow;
			account.Verified = DateTime.UtcNow;
			account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

			await context.Accounts.AddAsync(account);
			await context.SaveChangesAsync();

			return mapper.Map<AccountResponse>(account);
		}

		public async Task Delete(int id)
		{
			var account = await GetAccountById(id);
			context.Accounts.Remove(account);
			await context.SaveChangesAsync();	
		}

		public async Task ForgotPassword(ForgotPasswordRequest request, string origin)
		{
			var account = await context.Accounts.SingleOrDefaultAsync(x => x.Email == request.Email);

			if (account == null)
			{
				return;
			}

			account.ResetToken = await authService.GenerateResetToken();
			account.ResetTokenExpires = DateTime.UtcNow.AddDays(1);

			context.Accounts.Update(account);
			await context.SaveChangesAsync();

			SendPasswordResetEmail(account, origin);
		}

		public async Task<IEnumerable<AccountResponse>> GetAccounts()
		{
			var accounts = await context.Accounts
				.Select(a => mapper.Map<AccountResponse>(a))
				.AsNoTracking()
				.ToListAsync();
			return accounts;
		}

		public async Task<AccountResponse> GetById(int id)
		{
			var account = await GetAccountById(id);
			return mapper.Map<AccountResponse>(account);
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

		public async Task ResetPassword(ResetPasswordRequest request)
		{
			var account = await GetAccountByResetToken(request.Token);

			account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
			account.PasswordReset = DateTime.UtcNow;
			account.ResetToken = null;
			account.ResetTokenExpires = null;

			context.Accounts.Update(account);
			await context.SaveChangesAsync();
		}

		public async Task<AccountResponse> Update(int id, UpdateRequest model)
		{
			var account = await GetAccountById(id);
			var isEmailRegistered = await context.Accounts.AnyAsync(x => x.Email == model.Email);

			if (account.Email != model.Email && isEmailRegistered)
			{
				throw new DomainException($"Email '{model.Email}' is already registered for a different user");
			}

			if (!string.IsNullOrEmpty(model.Password))
			{
				account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
			}

			mapper.Map(model, account);
			account.Updated = DateTime.UtcNow;
			context.Accounts.Update(account);
			await context.SaveChangesAsync();

			return mapper.Map<AccountResponse>(account);
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

		private void SendPasswordResetEmail(Account account, string origin)
		{
			string message;

			if (!string.IsNullOrEmpty(origin))
			{
				var resetUrl = $"{origin}/account/reset-password?token={account.ResetToken}";
				message = $@"<p>Please click the below link to reset your password, the link will be valid for 1 day:</p>
                            <p><a href=""{resetUrl}"">{resetUrl}</a></p>";
			}
			else
			{
				message = $@"<p>Please use the below token to reset your password with the <code>/accounts/reset-password</code> api route:</p>
                            <p><code>{account.ResetToken}</code></p>";
			}

			var subject = "Sign-up Verification API - Reset password";
			var html = $@"<h4>Reset Password Email</h4>
                        {message}";

			mailService.Send(account.Email, subject, html);
		}

		private async Task<Account> GetAccountById(int id)
		{
			var account = await context.Accounts.FindAsync(id);

			if (account == null)
			{
				throw new DomainException("Account not found");
			}

			return account;
		}

		private async Task<Account> GetAccountByResetToken(string token)
		{
			var account = await context.Accounts.SingleOrDefaultAsync(x => x.ResetToken == token);

			if (account == null)
			{
				throw new DomainException("Invalid reset token");
			}

			return account;
		}
		#endregion
	}
}
