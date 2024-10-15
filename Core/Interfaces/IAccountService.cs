using Core.Requests;
using Core.Responses;

namespace Core.Interfaces
{
    public interface IAccountService
	{
		Task Register(RegisterRequest request, string origin);
		Task ForgotPassword(ForgotPasswordRequest request, string origin);
		Task ResetPassword(ResetPasswordRequest request);
		Task<IEnumerable<AccountResponse>> GetAccounts();
		Task<AccountResponse> GetById(int id);
		Task<AccountResponse> Create(CreateRequest model);
		Task<AccountResponse> Update(int id, UpdateRequest model);
		Task Delete(int id);
	}
}
