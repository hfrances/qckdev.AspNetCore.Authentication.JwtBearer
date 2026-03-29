using JwtBearerExample.Identity.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JwtBearerExample.Identity.Pages.Ui;

[AllowAnonymous]
public class IndexModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly JwtTokenService _jwtTokenService;

    public IndexModel(UserManager<IdentityUser> userManager, JwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    public string? BearerToken { get; private set; }

    public DateTime? ExpiresAtUtc { get; private set; }

    public async Task OnGetAsync()
    {
        if (!(User.Identity?.IsAuthenticated ?? false))
        {
            return;
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return;
        }

        var token = await _jwtTokenService.CreateTokenAsync(user);
        BearerToken = $"Bearer {token.AccessToken}";
        ExpiresAtUtc = token.ExpiresAtUtc;
    }
}
