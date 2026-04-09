using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;

namespace GpMnrega.Web.Controllers;

// ── GET /api/registers/register3 ─────────────────────────────────────────────
// Mirrors register3.aspx.cs exactly.
//
// Crawl flow (same multi-stage ViewState-based POST as original):
//   1. GET  SA_LoginReport.aspx?id=15               → initial ViewState/dynSalt
//   2. If finyear != appsettings finyear:
//        POST ddlFin change                         → updated ViewState
//   3. POST ddldist = dist_code                      → district drill-down
//   4. POST ddlblock = block_code                    → block drill-down
//   5. POST ddlpanchayat = panchayat_code            → panchayat drill-down
//   6. POST rbLoginLevel$1 (GP login level)
//   7. POST login = "Get Reports" (with from/to dates)
//   8. GET  IndexFrame.aspx                          → establish session
//   9. GET  Consolidate_pay_wrker.aspx               → return raw HTML
//
// Query params: finyear, from (DD/MM/YYYY), to (DD/MM/YYYY),
//               dist_code, block_code, panch
// ─────────────────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/registers")]
public class Register3Controller : ControllerBase
{
    private readonly ILogger<Register3Controller> _log;
    private readonly IConfiguration _config;
    private const string SA_URL = "https://mnregaweb4.nic.in/netnrega/SocialAudit/SA_LoginReport.aspx?id=15";

    public Register3Controller(ILogger<Register3Controller> log, IConfiguration config)
    {
        _log = log;
        _config = config;
    }

    [HttpGet("register3")]
    public async Task<IActionResult> Get(
        [FromQuery] string? finyear,
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] string? dist_code,
        [FromQuery] string? block_code,
        [FromQuery] string? panch)
    {
        try
        {
            string configFinyear = _config["finyear"] ?? "";

            // Step 1: Initial GET
            using var client1 = new HttpClient();
            var resp = await client1.GetAsync(SA_URL);
            string respContent = await resp.Content.ReadAsStringAsync();

            // Step 2: POST fin year change only if different from configured year
            if (finyear != configFinyear)
            {
                var finDict = FillRequest(respContent, "ctl00$ContentPlaceHolder1$ddlFin", finyear, "0", "0", "0", "0", "0");
                using var clientFin = new HttpClient();
                var finResp = await clientFin.PostAsync(SA_URL, new FormUrlEncodedContent(finDict));
                respContent = await finResp.Content.ReadAsStringAsync();
            }

            // Step 3: POST ddldist
            var distDict = FillRequest(respContent, "ctl00$ContentPlaceHolder1$ddldist", finyear, dist_code, "0", "0", "0", "0");
            using var client3 = new HttpClient();
            var res3 = await client3.PostAsync(SA_URL, new FormUrlEncodedContent(distDict));
            string res3Content = await res3.Content.ReadAsStringAsync();

            // Step 4: POST ddlblock
            var blockDict = FillRequest(res3Content, "ctl00$ContentPlaceHolder1$ddlblock", finyear, dist_code, block_code, "0", "0", "0");
            using var client4 = new HttpClient();
            var res4 = await client4.PostAsync(SA_URL, new FormUrlEncodedContent(blockDict));
            string res4Content = await res4.Content.ReadAsStringAsync();

            // Step 5: POST ddlpanchayat
            var panchDict = FillRequest(res4Content, "ctl00$ContentPlaceHolder1$ddlpanchayat", finyear, dist_code, block_code, panch, "0", "0");
            using var client5 = new HttpClient();
            var res5 = await client5.PostAsync(SA_URL, new FormUrlEncodedContent(panchDict));
            string res5Content = await res5.Content.ReadAsStringAsync();

            // Step 6: POST rbLoginLevel (select GP login level)
            var loginLevelDict = FillRequest(res5Content, "ctl00$ContentPlaceHolder1$rbLoginLevel$1", finyear, dist_code, block_code, panch, "0", "0");
            using var client6 = new HttpClient();
            var res6 = await client6.PostAsync(SA_URL, new FormUrlEncodedContent(loginLevelDict));
            string res6Content = await res6.Content.ReadAsStringAsync();

            // Steps 7-9: POST login → GET IndexFrame → GET Consolidate_pay_wrker
            var handler = new HttpClientHandler { CookieContainer = new System.Net.CookieContainer() };
            using var clientFinal = new HttpClient(handler);

            var loginDict = FillRequest(res6Content, "ctl00$ContentPlaceHolder1$login", finyear, dist_code, block_code, panch, from, to);
            var res7 = await clientFinal.PostAsync(SA_URL, new FormUrlEncodedContent(loginDict));
            await res7.Content.ReadAsStringAsync(); // consume

            await clientFinal.GetAsync("https://mnregaweb4.nic.in/netnrega/SocialAudit/IndexFrame.aspx");
            var consolidateResp = await clientFinal.GetAsync("https://mnregaweb4.nic.in/netnrega/SocialAudit/Consolidate_pay_wrker.aspx");
            string finalHtml = await consolidateResp.Content.ReadAsStringAsync();

            return Content(finalHtml, "text/html");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Register3 crawl failed");
            return StatusCode(500, "Error connecting NREGA DataBase.");
        }
    }

    // Mirrors register3.aspx.cs fillRequest() exactly
    private Dictionary<string, string> FillRequest(
        string resp, string eventtarget, string finyear,
        string dist, string block, string panchayat,
        string from, string to)
    {
        var doc = new HtmlDocument { OptionUseIdAttribute = true };
        doc.LoadHtml(resp);

        string dynSalt      = doc.GetElementbyId("ctl00_ContentPlaceHolder1_dynSalt")?.GetAttributeValue("value", "") ?? "";
        string viewstate    = doc.GetElementbyId("__VIEWSTATE")?.GetAttributeValue("value", "") ?? "";
        string eventVal     = doc.GetElementbyId("__EVENTVALIDATION")?.GetAttributeValue("value", "") ?? "";
        string viewstateGen = doc.GetElementbyId("__VIEWSTATEGENERATOR")?.GetAttributeValue("value", "") ?? "";

        var request = new Dictionary<string, string>
        {
            ["ctl00$ContentPlaceHolder1$ddlFin"]        = finyear,
            ["ctl00$ContentPlaceHolder1$ddldist"]       = dist,
            ["ctl00$ContentPlaceHolder1$ddlblock"]      = block,
            ["ctl00$ContentPlaceHolder1$ddlpanchayat"]  = panchayat,
            ["ctl00$ScriptManager1"]                    = "ctl00$UpdatePanel1|" + eventtarget,
            ["__EVENTARGUMENT"]                         = "",
            ["__LASTFOCUS"]                             = "",
            ["__VIEWSTATE"]                             = viewstate,
            ["__VIEWSTATEGENERATOR"]                    = viewstateGen,
            ["__VIEWSTATEENCRYPTED"]                    = "",
            ["__EVENTVALIDATION"]                       = eventVal,
            ["ctl00$ContentPlaceHolder1$dynSalt"]       = dynSalt
        };

        if (eventtarget == "ctl00$ContentPlaceHolder1$login")
        {
            request["ctl00$ContentPlaceHolder1$rbLoginLevel"]   = "1";
            request["ctl00$ContentPlaceHolder1$txtperiodFrom"]  = from;
            request["ctl00$ContentPlaceHolder1$txtperiodTo"]    = to;
            request["ctl00$ContentPlaceHolder1$login"]          = "Get Reports";
            request["__EVENTTARGET"]                            = "";
        }
        else
        {
            request["__EVENTTARGET"] = eventtarget;
        }

        if (eventtarget == "ctl00$ContentPlaceHolder1$rbLoginLevel$1")
            request["ctl00$ContentPlaceHolder1$rbLoginLevel"] = "1";

        return request;
    }
}
