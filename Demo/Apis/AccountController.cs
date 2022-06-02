using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Demo.Apis
{
    public class AccountController : ApiBase
    {
        [HttpGet]
        public IActionResult Get([FromServices] IHttpContextAccessor httpContextAccessor)
        {
            var httpContext = httpContextAccessor.HttpContext;

            var user = httpContext.User;

            return Ok(new { UserId = user.FindFirst(ClaimTypes.NameIdentifier), Role = user.FindFirst(ClaimTypes.Role), FullName = user.FindFirst(ClaimTypes.Name) });
        }

    }
}
