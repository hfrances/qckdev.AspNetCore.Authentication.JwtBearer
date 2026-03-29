using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JwtBearerExample.SingleSchemeWithRoles.Controllers
{
    [ApiController]
    [Route("jwt")]
    public sealed class JwtController : ControllerBase
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

        [HttpGet("roles/admin")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = Startup.PolicyAdminOnly)]
        public IActionResult RolesAdmin()
        {
            return Ok($"roles-admin-ok-{User?.Identity?.Name}");
        }

        [HttpGet("roles/support")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = Startup.PolicySupportOnly)]
        public IActionResult RolesSupport()
        {
            return Ok($"roles-support-ok-{User?.Identity?.Name}");
        }

        [HttpGet("roles/either")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = Startup.PolicyAdminOrSupport)]
        public IActionResult RolesEither()
        {
            return Ok($"roles-either-ok-{User?.Identity?.Name}");
        }

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
