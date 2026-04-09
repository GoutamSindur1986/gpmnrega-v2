using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;

namespace GpMnrega.Web.Controllers;

// ── GET /api/proxy/register3paymentmap ───────────────────────────────────────
// Mirrors register3paymentmap.aspx.cs exactly.
// Route matches what V2 cashbook.js already calls.
//
// Crawl flow:
//   1. GET PoIndexFrame.aspx (block index) → find citizen_html/funddisreport link
//   2. GET funddisreport.aspx              → expense/payment link listing
//   3. Find mustworkexpn.aspx link matching panchayat_code
//   4. GET mustworkexpn.aspx               → return raw HTML
//
// Query params: dist_code, panchayat_code, state_Code, block_code,
//               state_name, dist_name, block_name, fin_year, pname
// ─────────────────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/proxy")]
public class Register3PaymentMapController : ControllerBase
{
    private readonly ILogger<Register3PaymentMapController> _log;
    private const string NIC_BASE = "https://nregastrep.nic.in/netnrega/";

    public Register3PaymentMapController(ILogger<Register3PaymentMapController> log) => _log = log;

    [HttpGet("register3paymentmap")]
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

            // Find citizen_html/funddisreport link (same as original: start from index 70)
            string link = "";
            for (int i = 70; i < links.Count; i++)
            {
                link = links[i].Attributes["href"]?.Value?.Replace("../", NIC_BASE) ?? "";
                if (link.Contains("citizen_html/funddisreport.aspx?"))
                    break;
            }
            if (string.IsNullOrEmpty(link)) return StatusCode(500, "funddisreport link not found");

            // Step 2: GET funddisreport.aspx → expense link listing
            var panchyatResp = await client.GetAsync(link);
            string panchResp = await panchyatResp.Content.ReadAsStringAsync();

            doc = new HtmlDocument();
            doc.LoadHtml(panchResp);
            var expenseLinks = doc.DocumentNode.SelectNodes("//a");
            if (expenseLinks == null) return StatusCode(500, "Expense links not found");

            string finalExpenseLink = "";
            foreach (var item in expenseLinks)
            {
                // Relative path uses "../../" prefix (same as original)
                string href = item.Attributes["href"]?.Value ?? "";
                string itemLink = href.Replace("../../", NIC_BASE);
                var qs = System.Web.HttpUtility.ParseQueryString(new Uri(itemLink.StartsWith("http") ? itemLink : "http://placeholder/" + itemLink).Query);
                if (itemLink.Contains("mustworkexpn.aspx") && qs["panchayat_code"] == panchayat_code)
                {
                    finalExpenseLink = itemLink;
                    break;
                }
            }
            if (string.IsNullOrEmpty(finalExpenseLink)) return StatusCode(500, "mustworkexpn link not found");

            // Step 3: GET mustworkexpn.aspx → return raw HTML
            var finExp = await client.GetAsync(finalExpenseLink);
            string finalHtml = await finExp.Content.ReadAsStringAsync();

            return Content(finalHtml, "text/html");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Register3PaymentMap crawl failed");
            return StatusCode(500, "Error connecting NREGA DataBase.");
        }
    }
}
