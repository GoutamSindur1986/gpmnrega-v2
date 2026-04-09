using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace GpMnrega.Web.Controllers;

// ── GET /api/registers/wagecashwork ──────────────────────────────────────────
// Mirrors WageCashWork.aspx.cs exactly.
//
// Crawl flow (same multi-step as original):
//   1. GET PoIndexFrame.aspx (block-level index) → extract session cookie + emuster link
//   2. GET emuster_wagelist_rpt.aspx             → list of panchayats
//   3. Find panchayat row matching panchayat_code
//   4. GET the panchayat-specific wage list page → return raw HTML
//
// Query params: dist_code, block_code, panchayat_code, district_name, block_name,
//               panchayat_name, fin_year, type (optional, 9 = material)
// ─────────────────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/registers")]
public class WageCashWorkController : ControllerBase
{
    private readonly ILogger<WageCashWorkController> _log;
    private const string NIC_BASE = "https://nregastrep.nic.in/netnrega/";

    public WageCashWorkController(ILogger<WageCashWorkController> log) => _log = log;

    [HttpGet("wagecashwork")]
    public async Task<IActionResult> Get(
        [FromQuery] string? dist_code,
        [FromQuery] string? block_code,
        [FromQuery] string? panchayat_code,
        [FromQuery] string? district_name,
        [FromQuery] string? block_name,
        [FromQuery] string? panchayat_name,
        [FromQuery] string? fin_year,
        [FromQuery] string? type = null)
    {
        try
        {
            // Step 1: GET PoIndexFrame.aspx → session cookie + emuster link
            string indexUrl = NIC_BASE + $"Progofficer/PoIndexFrame.aspx?flag_debited=S&lflag=eng" +
                $"&District_Code={dist_code}&district_name={Uri.EscapeDataString(district_name)}" +
                $"&state_name=KARNATAKA&state_Code=15&finyear={fin_year}&check=1" +
                $"&block_name={Uri.EscapeDataString(block_name)}&Block_Code={block_code}";

            var req1 = (HttpWebRequest)WebRequest.Create(indexUrl);
            var resp1 = (HttpWebResponse)await Task.Factory.FromAsync(req1.BeginGetResponse, req1.EndGetResponse, null);
            string blockcontent = await new StreamReader(resp1.GetResponseStream()).ReadToEndAsync();
            string session = resp1.Headers.Get("Set-Cookie")?.Split('=')[1]?.Split(';')[0] ?? "";
            resp1.Close();

            var doc = new HtmlDocument();
            doc.LoadHtml(blockcontent);

            // Find emuster_wagelist_rpt.aspx link (same as original: start from index 50)
            string emusterlink = "";
            var blinks = doc.DocumentNode.SelectNodes("//a");
            if (blinks == null) return StatusCode(500, "Could not find links in PoIndexFrame");
            for (int i = 50; i < blinks.Count; i++)
            {
                var href = blinks[i].Attributes["href"]?.Value ?? "";
                emusterlink = href.Replace("../", NIC_BASE);
                if (emusterlink.Contains("/emuster_wagelist_rpt.aspx?"))
                    break;
            }

            if (string.IsNullOrEmpty(emusterlink))
                return StatusCode(500, "emuster link not found");

            // Step 2: GET emuster_wagelist_rpt.aspx with session cookie
            var req2 = (HttpWebRequest)WebRequest.Create(emusterlink);
            req2.CookieContainer = new CookieContainer();
            req2.CookieContainer.Add(new Cookie("ASP.NET_SessionId", session) { Domain = "nregastrep.nic.in" });
            req2.Method = "GET";
            var resp2 = (HttpWebResponse)await Task.Factory.FromAsync(req2.BeginGetResponse, req2.EndGetResponse, null);
            string emusterResp = await new StreamReader(resp2.GetResponseStream()).ReadToEndAsync();
            resp2.Close();

            doc = new HtmlDocument();
            doc.LoadHtml(emusterResp);

            // Step 3: Find panchayat link matching panchayat_code
            // type=9 → column 3, otherwise → column 12 (same as original)
            var musterlinks = type == "9"
                ? doc.DocumentNode.SelectNodes("//table[1]//tr//td[3]//a")
                : doc.DocumentNode.SelectNodes("//table[1]//tr//td[12]//a");

            if (musterlinks == null) return StatusCode(500, "No panchayat links found");

            string requestMustlink = "";
            foreach (var item in musterlinks)
            {
                var href = item.Attributes["href"]?.Value ?? "";
                requestMustlink = NIC_BASE + "state_html/" + href;
                var qs = System.Web.HttpUtility.ParseQueryString(new Uri(requestMustlink).Query);
                if (qs["panchayat_code"] == panchayat_code)
                    break;
            }

            if (string.IsNullOrEmpty(requestMustlink))
                return StatusCode(500, "Panchayat link not found");

            // Step 4: GET the panchayat wage list page → return raw HTML
            var req3 = (HttpWebRequest)WebRequest.Create(requestMustlink);
            req3.CookieContainer = new CookieContainer();
            req3.CookieContainer.Add(new Cookie("ASP.NET_SessionId", session) { Domain = "nregastrep.nic.in" });
            req3.Method = "GET";
            req3.Timeout = 50000;
            var resp3 = (HttpWebResponse)await Task.Factory.FromAsync(req3.BeginGetResponse, req3.EndGetResponse, null);
            string finalHtml = await new StreamReader(resp3.GetResponseStream()).ReadToEndAsync();
            resp3.Close();

            return Content(finalHtml, "text/html");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "WageCashWork crawl failed");
            return StatusCode(500, "Error connecting NREGA DataBase.");
        }
    }
}
