using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JwtBearerExample.SingleScheme.Controllers
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
