using Core.Requests;
using Core.Responses;

namespace Core.Interfaces
{
    public interface IAuthService
	{
		Task<AuthResponse> Authenticate(AuthRequest request, string ipAddress);
		Task<AuthResponse> RefreshToken(string token, string ipAddress);
		Task RevokeToken(string token, string ipAddress);
		Task ValidateResetToken(ValidateResetTokenRequest model);
		Task VerifyEmail(string token);
	}
}
