using Microsoft.AspNetCore.Mvc;

namespace JwtBearerExample.MultipleSchemes.Controllers
{
    /// <summary>
    /// Exposes the public endpoint for the multiple JWT schemes example.
    /// </summary>
    [ApiController]
    [Route("jwt")]
    public sealed class JwtController : ControllerBase
    {
        /// <summary>
        /// Returns a public response without authentication.
        /// </summary>
        /// <response code="200">Public endpoint reached successfully.</response>
        [HttpGet("public")]
        public IActionResult Public()
        {
            return Ok("public-ok");
        }
    }
}
