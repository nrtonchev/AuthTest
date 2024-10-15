using Core.Entities.Auth;
using Core.Entities.Models;

namespace Core.Interfaces
{
	public interface IJwtTokenUtils
	{
		int? ValidateToken(string token);
		string GenerateToken(Account account);
		RefreshToken GenerateRefreshToken(string ipAddress);
	}
}
