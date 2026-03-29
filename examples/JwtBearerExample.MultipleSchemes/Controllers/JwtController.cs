using Microsoft.AspNetCore.Mvc;

namespace JwtBearerExample.MultipleSchemes.Controllers
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
    }
}
