using JwtBearerExample.Identity.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtBearerExample.Identity.Controllers;

/// <summary>
/// Demonstrates role-based authorization endpoints backed by local Identity roles.
/// </summary>
[ApiController]
[Route("roles")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class RolesController : ControllerBase
{
    /// <summary>
    /// Returns a response only for users in the Admin role.
    /// </summary>
    /// <response code="200">User has the Admin role.</response>
    /// <response code="401">Missing or invalid Bearer token.</response>
    /// <response code="403">User is authenticated but does not have the Admin role.</response>
    [HttpGet("admin")]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    public IActionResult AdminOnly() => Ok("admin-ok");

    /// <summary>
    /// Returns a response only for users in the Support role.
    /// </summary>
    /// <response code="200">User has the Support role.</response>
    /// <response code="401">Missing or invalid Bearer token.</response>
    /// <response code="403">User is authenticated but does not have the Support role.</response>
    [HttpGet("support")]
    [Authorize(Policy = AuthPolicies.SupportOnly)]
    public IActionResult SupportOnly() => Ok("support-ok");

    /// <summary>
    /// Returns a response for users in Admin or Support role.
    /// </summary>
    /// <response code="200">User has at least one allowed role.</response>
    /// <response code="401">Missing or invalid Bearer token.</response>
    /// <response code="403">User is authenticated but has no allowed role.</response>
    [HttpGet("either")]
    [Authorize(Policy = AuthPolicies.AdminOrSupport)]
    public IActionResult AdminOrSupport() => Ok("either-ok");
}
