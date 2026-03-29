using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtBearerExample.MultipleSchemes.Controllers
{
    [ApiController]
    [Route("jwt")]
    public sealed class ProtectedController : ControllerBase
    {
        [HttpGet("protected")]
        [Authorize(AuthenticationSchemes = Startup.AUTHENTICATIONSCHEME_TOKEN)]
        public IActionResult Protected()
        {
            return Ok($"protected-ok-{User?.Identity?.Name}");
        }

        [HttpGet("protected/code")]
        [Authorize(AuthenticationSchemes = Startup.AUTHENTICATIONSCHEME_CODE)]
        public IActionResult ProtectedCode()
        {
            return Ok($"protected-code-ok-{User?.Identity?.Name}");
        }

        [HttpGet("protected/bearer")]
        [Authorize(AuthenticationSchemes = Startup.AUTHENTICATIONSCHEME_TOKEN)]
        public IActionResult ProtectedBearer()
        {
            return Ok($"protected-bearer-ok-{User?.Identity?.Name}");
        }
    }
}
