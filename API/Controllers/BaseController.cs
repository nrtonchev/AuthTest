using Core.Entities.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
public class BaseController : ControllerBase
{
    public Account Account => (Account)HttpContext.Items["Account"];
    public string Origin => Request.Headers["Origin"];
}