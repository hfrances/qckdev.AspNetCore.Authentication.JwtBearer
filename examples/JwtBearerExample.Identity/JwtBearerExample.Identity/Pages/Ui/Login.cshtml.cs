using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading;

namespace JwtBearerExample.Identity.Pages.Ui;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public LoginModel(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [BindProperty]
    public LoginInput Input { get; set; } = new()
    {
        Email = "admin@local.dev",
        Password = "Admin123!"
    };

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user is null)
        {
            ErrorMessage = "Invalid credentials.";
            return Page();
        }

        cancellationToken.ThrowIfCancellationRequested();
        var result = await _signInManager.PasswordSignInAsync(user, Input.Password, isPersistent: true, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            ErrorMessage = "Invalid credentials.";
            return Page();
        }

        return Redirect("/ui");
    }

    public sealed class LoginInput
    {
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
