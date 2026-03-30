using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace JwtBearerExample.MultipleSchemes.Controllers
{
    /// <summary>
    /// Issues JWT tokens for each configured authentication scheme.
    /// </summary>
    [ApiController]
    [Route("jwt/token")]
    public sealed class TokenController : ControllerBase
    {
        /// <summary>
        /// Generates a token for the Code scheme.
        /// </summary>
        /// <response code="200">Token generated successfully.</response>
        [HttpPost("code")]
        public async Task<IActionResult> Code([FromServices] IJwtGeneratorService tokenService, [FromBody] TokenRequest? request, CancellationToken cancellationToken)
        {
            var model = request ?? new TokenRequest();
            var token = await tokenService.CreateTokenAsync(
                Startup.AUTHENTICATIONSCHEME_CODE,
                model.UserName,
                model.Roles,
                new[] { new Claim("scope", "api.code") },
                cancellationToken);

            return Ok(token);
        }

        /// <summary>
        /// Generates a token for the Bearer scheme.
        /// </summary>
        /// <response code="200">Token generated successfully.</response>
        [HttpPost("bearer")]
        public async Task<IActionResult> Bearer([FromServices] IJwtGeneratorService tokenService, [FromBody] TokenRequest? request, CancellationToken cancellationToken)
        {
            var model = request ?? new TokenRequest();
            var token = await tokenService.CreateTokenAsync(
                Startup.AUTHENTICATIONSCHEME_TOKEN,
                model.UserName,
                model.Roles,
                new[] { new Claim("scope", "api.token") },
                cancellationToken);

            return Ok(token);
        }
    }
}
