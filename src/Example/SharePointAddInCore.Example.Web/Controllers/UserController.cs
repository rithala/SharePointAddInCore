using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SharePointAddInCore.Core.Authentication;
using SharePointAddInCore.Core.SharePointContext;

using System.Threading.Tasks;

namespace SharePointAddInCore.Example.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ISharePointContext _sharePointContext;

        public UserController(ISharePointContext sharePointContext)
        {
            _sharePointContext = sharePointContext;
        }
        [Authorize]
        [HttpGet("")]
        public async Task<IActionResult> Get()
        {
            return Ok(Request.HttpContext.User.GetUserAccessToken());
        }

        [HttpGet("app")]
        public async Task<IActionResult> GetAppOnly()
        {
            var appToken = await _sharePointContext.GetAppOnlyAccessTokenForSPHost();
            return Ok(appToken);
        }
    }
}
