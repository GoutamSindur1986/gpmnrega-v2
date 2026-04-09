using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GpMnrega.Web.Pages.Auth;

[Authorize]
public class HomeModel : PageModel
{
    // All geographic data is read directly from auth claims in Home.cshtml.
    // Claims are populated during login from the AuthenticateUser SP (which JOINs
    // the user table with panchayat/taluk/district tables and returns all geo data).
    // No extra DB call needed here.
    public void OnGet() { }
}
