using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JwtBearerExample.SingleSchemeWithRoles.Controllers
{
    /// <summary>
    /// Demonstrates JWT endpoints with role-based authorization policies.
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
        /// Returns a response only for users in the Admin role.
        /// </summary>
        /// <response code="200">User has the Admin role.</response>
        /// <response code="401">Missing or invalid Bearer token.</response>
        /// <response code="403">User is authenticated but does not have the Admin role.</response>
        [HttpGet("roles/admin")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = Startup.PolicyAdminOnly)]
        public IActionResult RolesAdmin()
        {
            return Ok($"roles-admin-ok-{User?.Identity?.Name}");
        }

        /// <summary>
        /// Returns a response only for users in the Support role.
        /// </summary>
        /// <response code="200">User has the Support role.</response>
        /// <response code="401">Missing or invalid Bearer token.</response>
        /// <response code="403">User is authenticated but does not have the Support role.</response>
        [HttpGet("roles/support")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = Startup.PolicySupportOnly)]
        public IActionResult RolesSupport()
        {
            return Ok($"roles-support-ok-{User?.Identity?.Name}");
        }

        /// <summary>
        /// Returns a response for users in either Admin or Support role.
        /// </summary>
        /// <response code="200">User has at least one allowed role.</response>
        /// <response code="401">Missing or invalid Bearer token.</response>
        /// <response code="403">User is authenticated but has no allowed role.</response>
        [HttpGet("roles/either")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = Startup.PolicyAdminOrSupport)]
        public IActionResult RolesEither()
        {
            return Ok($"roles-either-ok-{User?.Identity?.Name}");
        }

        /// <summary>
        /// Generates a JWT token including requested roles.
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
