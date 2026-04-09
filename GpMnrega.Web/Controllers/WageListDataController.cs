using Microsoft.AspNetCore.Mvc;

namespace GpMnrega.Web.Controllers;

// ── POST /api/proxy/wagelistdata ─────────────────────────────────────────────
// Replaces the original ../api/wagelistdata POST endpoint.
//
// The JS (doAjaxRequest in cashbook.js / register5.js) POSTs a raw NIC URL
// string as the request body and expects back the HTML of that NIC page.
// This is a simple passthrough proxy — fetch the URL, return the HTML.
//
// Used by: generateFTOWageListForm, generateFTOMaterialListForm, getRegister5Data
//          when fetching individual FTO transaction or work detail pages.
// ─────────────────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/proxy")]
public class WageListDataController : ControllerBase
{
    private readonly ILogger<WageListDataController> _log;
    private const string UA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36";

    public WageListDataController(ILogger<WageListDataController> log) => _log = log;

    [HttpPost("wagelistdata")]
    public async Task<IActionResult> Post()
    {
        try
        {
            // Read the NIC URL from the POST body (plain text, same as original)
            string nicUrl;
            using (var reader = new StreamReader(Request.Body))
                nicUrl = (await reader.ReadToEndAsync()).Trim();

            if (string.IsNullOrEmpty(nicUrl))
                return BadRequest("URL is required in request body");

            // Validate it's a NIC URL for safety
            if (!nicUrl.StartsWith("https://nregastrep.nic.in/") &&
                !nicUrl.StartsWith("https://mnregaweb4.nic.in/") &&
                !nicUrl.StartsWith("http://nregastrep.nic.in/") &&
                !nicUrl.StartsWith("http://mnregaweb4.nic.in/"))
            {
                return BadRequest("Only NIC URLs are allowed");
            }

            using var handler = new HttpClientHandler { AllowAutoRedirect = true };
            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent", UA);

            var response = await client.GetAsync(nicUrl);
            string html = await response.Content.ReadAsStringAsync();

            return Content(html, "text/html");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "WageListData proxy failed");
            return StatusCode(500, "Error fetching NIC data.");
        }
    }
}
