using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GpMnrega.Web.Pages.AuthAgency;

[Authorize]
public class DeptHomeModel : PageModel { }
