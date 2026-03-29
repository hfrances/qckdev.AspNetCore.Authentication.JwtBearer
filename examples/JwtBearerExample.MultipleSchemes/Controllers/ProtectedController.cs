using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtBearerExample.MultipleSchemes.Controllers
{
    /// <summary>
    /// Exposes JWT-protected endpoints for specific or combined schemes.
    /// </summary>
    [ApiController]
    [Route("jwt")]
    public sealed class ProtectedController : ControllerBase
    {
        /// <summary>
        /// Returns a protected response for the Bearer scheme.
        /// </summary>
        /// <response code="200">Authenticated request completed successfully.</response>
        /// <response code="401">Missing or invalid token for the Bearer scheme.</response>
        [HttpGet("protected")]
        [Authorize(AuthenticationSchemes = Startup.AUTHENTICATIONSCHEME_TOKEN)]
        public IActionResult Protected()
        {
            return Ok($"protected-ok-{User?.Identity?.Name}");
        }

        /// <summary>
        /// Returns a protected response for the Code scheme.
        /// </summary>
        /// <response code="200">Authenticated request completed successfully.</response>
        /// <response code="401">Missing or invalid token for the Code scheme.</response>
        [HttpGet("protected/code")]
        [Authorize(AuthenticationSchemes = Startup.AUTHENTICATIONSCHEME_CODE)]
        public IActionResult ProtectedCode()
        {
            return Ok($"protected-code-ok-{User?.Identity?.Name}");
        }

        /// <summary>
        /// Returns a protected response for the Bearer scheme.
        /// </summary>
        /// <response code="200">Authenticated request completed successfully.</response>
        /// <response code="401">Missing or invalid token for the Bearer scheme.</response>
        [HttpGet("protected/bearer")]
        [Authorize(AuthenticationSchemes = Startup.AUTHENTICATIONSCHEME_TOKEN)]
        public IActionResult ProtectedBearer()
        {
            return Ok($"protected-bearer-ok-{User?.Identity?.Name}");
        }

        /// <summary>
        /// Returns a protected response for requests authenticated with Code or Bearer scheme.
        /// </summary>
        /// <response code="200">Authenticated request completed successfully.</response>
        /// <response code="401">Missing or invalid token for both configured schemes.</response>
        [HttpGet("protected/either")]
        [Authorize(AuthenticationSchemes = Startup.AUTHENTICATIONSCHEME_CODE + "," + Startup.AUTHENTICATIONSCHEME_TOKEN)]
        public IActionResult ProtectedEither()
        {
            return Ok($"protected-either-ok-{User?.Identity?.Name}");
        }
    }
}
