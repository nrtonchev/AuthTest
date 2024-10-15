using API.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
    [Authorize]
	public class AccountsController : ControllerBase
	{
        public AccountsController()
        {
            
        }
    }
}
