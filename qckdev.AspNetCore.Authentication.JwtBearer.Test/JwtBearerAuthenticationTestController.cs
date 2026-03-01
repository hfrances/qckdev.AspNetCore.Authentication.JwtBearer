using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace qckdev.AspNetCore.Authentication.JwtBearer.Test
{
    [ApiController]
    [Route("jwt")]
    public sealed class JwtBearerAuthenticationTestController : ControllerBase
    {
        [HttpGet("public")]
        public IActionResult Public()
        {
            return Ok("public-ok");
        }

        [HttpGet("protected")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Protected()
        {
            return Ok($"protected-ok-{User?.Identity?.Name}");
        }
    }
}
