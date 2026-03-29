using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JwtBearerExample.Identity.Pages.Ui;

[AllowAnonymous]
public class LogoutModel : PageModel
{
    public IActionResult OnGet()
    {
        return Redirect("/ui");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        return Redirect("/ui");
    }
}
