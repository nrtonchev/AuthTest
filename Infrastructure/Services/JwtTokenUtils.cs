using Core.Entities.Auth;
using Core.Entities.Models;
using Core.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services
{
	public class JwtTokenUtils : IJwtTokenUtils
	{
		private readonly ApplicationContext appContext;
		private readonly AppSettings appSettings;

		public JwtTokenUtils(ApplicationContext appContext, IOptions<AppSettings> appSettings)
        {
			this.appContext = appContext;
			this.appSettings = appSettings.Value;
		}

        public string GenerateToken(Account account)
		{
			// Generate token which will be valid for 15 minutes
			var tokenHandler = new JwtSecurityTokenHandler();
			var key = Encoding.ASCII.GetBytes(this.appSettings.Secret);

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new[] { new Claim("id", account.Id.ToString()) }),
				Expires = DateTime.UtcNow.AddMinutes(15),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.EcdsaSha256Signature)
			};

			var token = tokenHandler.CreateToken(tokenDescriptor);

			return tokenHandler.WriteToken(token);
		}

		public RefreshToken GenerateRefreshToken(string ipAddress)
		{
			var refreshToken = new RefreshToken
			{
				Token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64)),
				Expires = DateTime.UtcNow.AddDays(7),
				Created = DateTime.UtcNow,
				CreatedByIp = ipAddress
			};

			// Check for token uniqueness
			var tokenIsUnique = !appContext.Accounts.Any(x => x.RefreshTokens
				.Any(r => r.Token == refreshToken.Token));

			if (!tokenIsUnique)
			{
				return GenerateRefreshToken(ipAddress);
			}
			
			return refreshToken;
		}

		public int? ValidateToken(string token)
		{
			if (token == null)
			{
				return null;
			}

			var tokenHandler = new JwtSecurityTokenHandler();
			var key = Encoding.ASCII.GetBytes(appSettings.Secret);

			try
			{
				tokenHandler.ValidateToken(token, new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(key),
					ValidateIssuer = true,
					ValidateAudience = true,
					ClockSkew = TimeSpan.Zero
				}, out SecurityToken validatedToken);

				var jwtToken = (JwtSecurityToken)validatedToken;
				var accountId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

				// Return account Id if token verification is successful
				return accountId;
			}
			catch
			{
				// Return null if verification is not successful
				return null;
			}
		}
	}
}
