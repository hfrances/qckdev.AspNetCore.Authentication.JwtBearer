using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JwtBearerExample.Identity.Pages.Ui;

[Authorize]
public class MeModel : PageModel
{
    public void OnGet()
    {
    }
}
