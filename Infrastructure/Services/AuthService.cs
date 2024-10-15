using AutoMapper;
using Core;
using Core.Entities.Auth;
using Core.Entities.Models;
using Core.Interfaces;
using Core.Requests;
using Core.Responses;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services
{
    public class AuthService : IAuthService
	{
		private readonly ApplicationContext context;
		private readonly IJwtTokenUtils tokenUtils;
		private readonly IMapper mapper;
		private readonly AppSettings appSettings;

		public AuthService(ApplicationContext context, IJwtTokenUtils tokenUtils, IMapper mapper, IOptions<AppSettings> appSettings)
        {
			this.context = context;
			this.tokenUtils = tokenUtils;
			this.mapper = mapper;
			this.appSettings = appSettings.Value;
		}

        public async Task<AuthResponse> Authenticate(AuthRequest request, string ipAddress)
		{
			var account = await context.Accounts.SingleOrDefaultAsync(x => x.Email == request.Email);
			var isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, account.PasswordHash);

			if (account == null || !account.IsVerified  || !isValidPassword)
			{
				throw new DomainException("Incorrect email and/or password");
			}

			// Generate jwt token and refresh token
			var jwtToken = tokenUtils.GenerateToken(account);
			var refreshToken = tokenUtils.GenerateRefreshToken(ipAddress);
			account.RefreshTokens.Add(refreshToken);

			RemoveOldRefreshTokens(account);

			context.Update(account);
			await context.SaveChangesAsync();

			var response = mapper.Map<AuthResponse>(account);
			response.JwtToken = jwtToken;
			response.RefreshToken = refreshToken.Token;

			return response;
		}

		public async Task<AuthResponse> RefreshToken(string token, string ipAddress)
		{
			var account = await GetAccountByRefreshToken(token);
			var refreshToken = account.RefreshTokens.Single(x => x.Token == token);

			if (refreshToken.IsRevoked)
			{
				var revokeReason = $"Attempted reuse of revoked ancestor token: {token}";
				RevokeDescendatnRefreshTokens(refreshToken, account, ipAddress, revokeReason);
				context.Update(account);
				await context.SaveChangesAsync();
			}

			if (!refreshToken.IsActive)
			{
				throw new DomainException("Invalid refresh token");
			}

			var newRefreshToken = RotateRefreshToken(refreshToken, ipAddress);
			account.RefreshTokens.Add(newRefreshToken);

			RemoveOldRefreshTokens(account);

			context.Update(account);
			await context.SaveChangesAsync();

			var jwtToken = tokenUtils.GenerateToken(account);

			var response = mapper.Map<AuthResponse>(account);
			response.JwtToken = jwtToken;
			response.RefreshToken = refreshToken.Token;

			return response;
		}

		public async Task RevokeToken(string token, string ipAddress)
		{
			var account = await GetAccountByRefreshToken(token);
			var refreshToken = account.RefreshTokens.Single(x => x.Token == token);

			if (!refreshToken.IsActive)
			{
				throw new DomainException("Invalid refresh token");
			}

			var revokeReason = "Revoked without replacement";
			RevokeRefreshToken(refreshToken, ipAddress, revokeReason);
			context.Update(account);
			await context.SaveChangesAsync();
		}

		public async Task ValidateResetToken(ValidateResetTokenRequest model)
		{
			await GetAccountByResetToken(model.Token);
		}

		public async Task VerifyEmail(string token)
		{
			var account = await context.Accounts.SingleOrDefaultAsync(x => x.VerificationToken == token);

			if (account == null)
			{
				throw new DomainException("Verification failed");
			}

			account.Verified = DateTime.UtcNow;
			account.VerificationToken = null;

			context.Accounts.Update(account);
			await context.SaveChangesAsync();
		}

		#region Private members
		private void RemoveOldRefreshTokens(Account account)
		{
			account.RefreshTokens.RemoveAll(x => !x.IsActive && x.Created.AddDays(appSettings.RefreshTokenExpiry) <= DateTime.UtcNow);
		}

		private async Task<Account> GetAccountByRefreshToken(string refreshToken)
		{
			var account = await context.Accounts.SingleOrDefaultAsync(x => x.RefreshTokens
				.Any(r => r.Token == refreshToken));

			if (account == null)
			{
				throw new DomainException("Invalid refresh token");
			}

			return account;
		}

		private void RevokeDescendatnRefreshTokens(RefreshToken refreshToken, Account account, string ipAddress, string reason)
		{
			if (!string.IsNullOrEmpty(refreshToken.ReplacedByToken))
			{
				var childToken = account.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken.ReplacedByToken);
				if (childToken.IsActive)
				{
					RevokeRefreshToken(childToken, ipAddress, reason);
				}
				else
				{
					RevokeDescendatnRefreshTokens(childToken, account, ipAddress, reason);
				}
			}
		}

		private void RevokeRefreshToken(RefreshToken token, string ipAddress, string reason = null, string replacedByToken = null)
		{
			token.Revoked = DateTime.UtcNow;
			token.RevokedByIp = ipAddress;
			token.ReasonRevoked = reason;
			token.ReplacedByToken = replacedByToken;
		}

		private RefreshToken RotateRefreshToken(RefreshToken refreshToken, string ipAddress)
		{
			var newRefreshToken = tokenUtils.GenerateRefreshToken(ipAddress);
			var revokeReason = "Replaced by new token";
			RevokeRefreshToken(refreshToken, ipAddress, revokeReason, newRefreshToken.Token);
			return newRefreshToken;
		}

		private async Task<Account> GetAccountByResetToken(string token)
		{
			var account = await context.Accounts.SingleOrDefaultAsync(x => x.ResetToken == token
				&& x.ResetTokenExpires > DateTime.UtcNow);

			if (account == null)
			{
				throw new DomainException("Invalid reset token");
			}

			return account;
		}
		#endregion
	}
}
