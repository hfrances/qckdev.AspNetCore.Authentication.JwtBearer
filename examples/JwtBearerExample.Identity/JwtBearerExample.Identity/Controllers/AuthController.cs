using JwtBearerExample.Identity.Contracts;
using JwtBearerExample.Identity.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JwtBearerExample.Identity.Controllers;

/// <summary>
/// Provides local authentication endpoints for public access, token issuance, and identity inspection.
/// </summary>
[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    /// <summary>
    /// Returns a public response without authentication.
    /// </summary>
    /// <response code="200">Public endpoint reached successfully.</response>
    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult Public() => Ok("public-ok");

    /// <summary>
    /// Validates user credentials and issues a JWT token.
    /// </summary>
    /// <response code="200">Token generated successfully.</response>
    /// <response code="401">Invalid email or password.</response>
    [HttpPost("token")]
    [AllowAnonymous]
    public async Task<IActionResult> Token(
        [FromBody] LoginRequest request,
        [FromServices] UserManager<IdentityUser> userManager,
        [FromServices] SignInManager<IdentityUser> signInManager,
        [FromServices] JwtTokenService tokenService)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Unauthorized("invalid-credentials");
        }

        var check = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!check.Succeeded)
        {
            return Unauthorized("invalid-credentials");
        }

        var token = await tokenService.CreateTokenAsync(user);
        return Ok(token);
    }

    /// <summary>
    /// Returns current authenticated user information and roles from JWT claims.
    /// </summary>
    /// <response code="200">User information returned successfully.</response>
    /// <response code="401">Missing or invalid Bearer token.</response>
    [HttpGet("whoami")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult WhoAmI()
    {
        var userName = User.Identity?.Name
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue(ClaimTypes.Email)
            ?? "unknown";

        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).Distinct().ToArray();
        return Ok(new { userName, roles });
    }
}
