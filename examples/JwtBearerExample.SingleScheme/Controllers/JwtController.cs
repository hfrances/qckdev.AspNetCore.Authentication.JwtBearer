using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JwtBearerExample.SingleScheme.Controllers
{
    /// <summary>
    /// Demonstrates public, protected, and token issuance endpoints for one JWT scheme.
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

        /// <summary>
        /// Returns a protected response for valid Bearer tokens.
        /// </summary>
        /// <response code="200">Authenticated request completed successfully.</response>
        /// <response code="401">Missing or invalid Bearer token.</response>
        [HttpGet("protected")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Protected()
        {
            return Ok($"protected-ok-{User?.Identity?.Name}");
        }

        /// <summary>
        /// Generates a JWT token using the configured single Bearer scheme.
        /// </summary>
        /// <response code="200">Token generated successfully.</response>
        [HttpPost("token")]
        public async Task<IActionResult> Token([FromServices] IJwtGeneratorService tokenService, [FromBody] TokenRequest? request)
        {
            var model = request ?? new TokenRequest();
            var token = await tokenService.CreateTokenAsync(
                JwtBearerDefaults.AuthenticationScheme,
                model.UserName,
                model.Roles,
                new[] { new Claim("scope", "api.read") });

            return Ok(token);
        }
    }
}
