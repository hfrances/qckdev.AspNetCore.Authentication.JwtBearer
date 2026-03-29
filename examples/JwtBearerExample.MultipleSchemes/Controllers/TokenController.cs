using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JwtBearerExample.MultipleSchemes.Controllers
{
    [ApiController]
    [Route("jwt/token")]
    public sealed class TokenController : ControllerBase
    {
        [HttpPost("code")]
        public async Task<IActionResult> Code([FromServices] IJwtGeneratorService tokenService, [FromBody] TokenRequest? request)
        {
            var model = request ?? new TokenRequest();
            var token = await tokenService.CreateTokenAsync(
                Startup.AUTHENTICATIONSCHEME_CODE,
                model.UserName,
                model.Roles,
                new[] { new Claim("scope", "api.code") });

            return Ok(token);
        }

        [HttpPost("bearer")]
        public async Task<IActionResult> Bearer([FromServices] IJwtGeneratorService tokenService, [FromBody] TokenRequest? request)
        {
            var model = request ?? new TokenRequest();
            var token = await tokenService.CreateTokenAsync(
                Startup.AUTHENTICATIONSCHEME_TOKEN,
                model.UserName,
                model.Roles,
                new[] { new Claim("scope", "api.token") });

            return Ok(token);
        }
    }
}
