using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace GpMnrega.Web.Controllers;

// ── GET /api/registers/assetscompleted ───────────────────────────────────────
// Mirrors assetscompleted.aspx.cs exactly.
//
// Crawl flow:
//   1. GET homestciti.aspx (Karnataka citizen page)
//   2. Determine currentFin from today's date
//   3. If finyear == currentFin:
//        Parse the response directly → find "Assets Created" link
//      Else:
//        POST fin_year change → find "Assets Created" link
//   4. GET "Assets Created" district listing → return raw HTML
//
// Query params: fin_year, dist_code
// ─────────────────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/registers")]
public class AssetsCompletedController : ControllerBase
{
    private readonly ILogger<AssetsCompletedController> _log;
    private const string PRIMARY_URL = "https://nregastrep.nic.in/netnrega/homestciti.aspx?state_code=15&state_name=KARNATAKA&lflag=eng&labels=labels";
    private const string NIC_BASE    = "https://nregastrep.nic.in/netnrega/";

    public AssetsCompletedController(ILogger<AssetsCompletedController> log) => _log = log;

    [HttpGet("assetscompleted")]
    public async Task<IActionResult> Get(
        [FromQuery] string? fin_year,
        [FromQuery] string? dist_code)
    {
        try
        {
            var handler = new HttpClientHandler
            {
                CookieContainer = new System.Net.CookieContainer(),
                UseCookies = true
            };
            using var client = new HttpClient(handler);

            // Step 1: GET homestciti.aspx
            var response = await client.GetAsync(PRIMARY_URL);
            string stateResponse = await response.Content.ReadAsStringAsync();

            // Step 2: Determine currentFin (same logic as original)
            string currentFin = DateTime.Now.Month < 3
                ? $"{DateTime.Now.Year - 1}-{DateTime.Now.Year}"
                : $"{DateTime.Now.Year}-{DateTime.Now.Year + 1}";

            string link = "";

            if (fin_year == currentFin)
            {
                // Step 3a: Find "Assets Created" from current page (start from index 325)
                var document = new HtmlDocument();
                document.LoadHtml(stateResponse);
                link = FindAssetsCreatedLink(document);
            }
            else
            {
                // Step 3b: POST fin_year change
                var document = new HtmlDocument();
                document.LoadHtml(stateResponse);
                string vs  = document.GetElementbyId("__VIEWSTATE")?.GetAttributeValue("value", "") ?? "";
                string vsg = document.GetElementbyId("__VIEWSTATEGENERATOR")?.GetAttributeValue("value", "") ?? "";
                string ev  = document.GetElementbyId("__EVENTVALIDATION")?.GetAttributeValue("value", "") ?? "";

                string finResp = $"__EVENTTARGET=fin_year&__EVENTARGUMENT=&__LASTFOCUS=" +
                                 $"&__VIEWSTATE={Uri.EscapeDataString(vs)}" +
                                 $"&__VIEWSTATEGENERATOR={vsg}" +
                                 $"&__SCROLLPOSITIONX=0&__SCROLLPOSITIONY=400" +
                                 $"&__VIEWSTATEENCRYPTED=&__EVENTVALIDATION={Uri.EscapeDataString(ev)}" +
                                 $"&fin_year={Uri.EscapeDataString(fin_year)}";

                var content1 = new StringContent(finResp, Encoding.UTF8, "application/x-www-form-urlencoded");
                var stateResp = await client.PostAsync(PRIMARY_URL, content1);
                string stateRespContent = await stateResp.Content.ReadAsStringAsync();

                document = new HtmlDocument();
                document.LoadHtml(stateRespContent);
                link = FindAssetsCreatedLink(document);
            }

            if (string.IsNullOrEmpty(link))
                return StatusCode(500, "Assets Created link not found");

            // Step 4: GET district listing → return raw HTML
            var distResponse = await client.GetAsync(link);
            string finalHtml = await distResponse.Content.ReadAsStringAsync();

            return Content(finalHtml, "text/html");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "AssetsCompleted crawl failed");
            return StatusCode(500, "Error connecting NREGA DataBase.");
        }
    }

    // Find "Assets Created" link starting from index 325 (same as original)
    private static string FindAssetsCreatedLink(HtmlDocument document)
    {
        var links = document.DocumentNode.SelectNodes("//a");
        if (links == null) return "";
        for (int a = 325; a < links.Count; a++)
        {
            if (links[a].InnerText.Trim() == "Assets Created")
                return "https://nregastrep.nic.in/netnrega/" + links[a].Attributes["href"]?.Value;
        }
        return "";
    }
}
