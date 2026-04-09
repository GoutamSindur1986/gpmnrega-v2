using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;

namespace GpMnrega.Web.Controllers;

// ── GET /api/proxy/register3workmap ──────────────────────────────────────────
// Mirrors Register3Workmap.aspx.cs exactly.
// Route matches what V2 cashbook.js already calls.
//
// Crawl flow:
//   1. GET PoIndexFrame.aspx (block index) → find emuster_wagelist_rpt link
//   2. GET emuster_wagelist_rpt.aspx       → panchayat listing (col 3 = work register)
//   3. Find panchayat link matching panchayat_code (col 3 = work summary)
//   4. GET panchayat page → return raw HTML
//
// Query params: dist_code, panchayat_code, state_Code, block_code,
//               state_name, dist_name, block_name, fin_year, pname
// ─────────────────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/proxy")]
public class Register3WorkmapController : ControllerBase
{
    private readonly ILogger<Register3WorkmapController> _log;
    private const string NIC_BASE = "https://nregastrep.nic.in/netnrega/";

    public Register3WorkmapController(ILogger<Register3WorkmapController> log) => _log = log;

    [HttpGet("register3workmap")]
    public async Task<IActionResult> Get(
        [FromQuery] string? dist_code,
        [FromQuery] string? panchayat_code,
        [FromQuery] string? state_Code,
        [FromQuery] string? block_code,
        [FromQuery] string? state_name,
        [FromQuery] string? dist_name,
        [FromQuery] string? block_name,
        [FromQuery] string? fin_year,
        [FromQuery] string? pname)
    {
        try
        {
            using var client = new HttpClient();

            // Step 1: GET PoIndexFrame.aspx
            string indexUrl = NIC_BASE +
                $"Progofficer/PoIndexFrame.aspx?flag_debited=S&lflag=eng" +
                $"&District_Code={dist_code}&district_name={Uri.EscapeDataString(dist_name)}" +
                $"&state_name=KARNATAKA&state_Code=15&finyear={fin_year}&check=1" +
                $"&block_name={Uri.EscapeDataString(block_name)}&Block_Code={block_code}";

            var message = await client.GetAsync(indexUrl);
            string res = await message.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(res);
            var links = doc.DocumentNode.SelectNodes("//a");
            if (links == null) return StatusCode(500, "No links in PoIndexFrame");

            // Find emuster_wagelist_rpt.aspx (same as original: start from index 50)
            string link = "";
            for (int i = 50; i < links.Count; i++)
            {
                link = links[i].Attributes["href"]?.Value?.Replace("../", NIC_BASE) ?? "";
                if (link.Contains("emuster_wagelist_rpt.aspx?"))
                    break;
            }
            if (string.IsNullOrEmpty(link)) return StatusCode(500, "emuster link not found");

            // Step 2: GET emuster_wagelist_rpt.aspx
            var panchyatResp = await client.GetAsync(link);
            string panchResp = await panchyatResp.Content.ReadAsStringAsync();

            doc = new HtmlDocument();
            doc.LoadHtml(panchResp);

            // Column 3 contains the work summary link (same as original)
            var panchlinks = doc.DocumentNode.SelectNodes("//center[1]//table[1]//tr//td[3]//a");
            if (panchlinks == null) return StatusCode(500, "Panchayat links not found");

            string finalPachLink = "";
            foreach (var panchlink in panchlinks)
            {
                var plink = NIC_BASE + "state_html/" + panchlink.Attributes["href"]?.Value;
                var qs = System.Web.HttpUtility.ParseQueryString(new Uri(plink).Query);
                if (qs["panchayat_code"] == panchayat_code)
                {
                    finalPachLink = plink;
                    break;
                }
            }
            if (string.IsNullOrEmpty(finalPachLink)) return StatusCode(500, "Panchayat link not found");

            // Step 3: GET panchayat work list page → return raw HTML
            var finalResp = await client.GetAsync(finalPachLink);
            string finalHtml = await finalResp.Content.ReadAsStringAsync();

            return Content(finalHtml, "text/html");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Register3Workmap crawl failed");
            return StatusCode(500, "Error connecting NREGA DataBase.");
        }
    }
}
