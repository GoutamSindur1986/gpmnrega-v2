using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;

namespace GpMnrega.Web.Controllers;

// ── GET /api/registers/ftosigndetails ────────────────────────────────────────
// Mirrors ftosigndetails.aspx.cs exactly.
//
// Crawl flow (same multi-step as original, identical to MaterialFTOController
// EXCEPT step 3 selects Wage type instead of Material):
//   1. GET homestciti.aspx → session cookie + find FTO/FTOReport.aspx link
//      (POST fin_year change if needed)
//   2. GET FTOReport.aspx with session → drill down district → block → panchayat
//      NOTE: No RBtnLst POST (Wage is the default type)
//   3. Return panchayat FTO HTML
//
// Query params: district_code, block_code, panchayat_code, district_name,
//               block_name, panchayat_name, fin_year
// ─────────────────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/registers")]
public class FtoSignDetailsController : ControllerBase
{
    private readonly ILogger<FtoSignDetailsController> _log;
    private const string PRIMARY_URL = "https://nregastrep.nic.in/netnrega/homestciti.aspx?state_code=15&state_name=KARNATAKA&lflag=eng&labels=labels";
    private const string FTO_BASE    = "https://nregastrep.nic.in/netnrega/FTO/";

    public FtoSignDetailsController(ILogger<FtoSignDetailsController> log) => _log = log;

    [HttpGet("ftosigndetails")]
    public async Task<IActionResult> Get(
        [FromQuery] string? district_code,
        [FromQuery] string? block_code,
        [FromQuery] string? panchayat_code,
        [FromQuery] string? district_name,
        [FromQuery] string? block_name,
        [FromQuery] string? panchayat_name,
        [FromQuery] string? fin_year)
    {
        try
        {
            using var client = new HttpClient();

            // Step 1: GET homestciti.aspx
            var primaryResp = await client.GetAsync(PRIMARY_URL);
            string primaryContent = await primaryResp.Content.ReadAsStringAsync();
            string session = primaryResp.Headers.GetValues("Set-Cookie").FirstOrDefault()?.Split('=')[1]?.Split(';')[0] ?? "";

            var doc = new HtmlDocument();
            doc.LoadHtml(primaryContent);

            // Determine currentFin (same as original — reads from page <select> if available)
            string currentFin;
            var selectNode = doc.DocumentNode.SelectSingleNode("//select[@id='fin_year']");
            if (selectNode != null)
                currentFin = selectNode.SelectSingleNode(".//option[@selected]")?.InnerText ?? "";
            else
                currentFin = DateTime.Now.Month < 4
                    ? $"{DateTime.Now.Year - 1}-{DateTime.Now.Year}"
                    : $"{DateTime.Now.Year}-{DateTime.Now.Year + 1}";

            string ftolink = "";

            if (fin_year == currentFin)
            {
                // Find FTO link directly (start from index 250)
                var links = doc.DocumentNode.SelectNodes("//a");
                if (links != null)
                    for (int i = 250; i < links.Count; i++)
                    {
                        var href = links[i].Attributes["href"]?.Value ?? "";
                        ftolink = "https://nregastrep.nic.in/netnrega/" + href;
                        if (href.Contains("FTO/FTOReport.aspx?"))
                            break;
                    }
            }
            else
            {
                // POST fin_year change
                string vs  = doc.GetElementbyId("__VIEWSTATE")?.GetAttributeValue("value", "") ?? "";
                string vsg = doc.GetElementbyId("__VIEWSTATEGENERATOR")?.GetAttributeValue("value", "") ?? "";
                string ev  = doc.GetElementbyId("__EVENTVALIDATION")?.GetAttributeValue("value", "") ?? "";

                string finBody = $"__EVENTTARGET=fin_year&__EVENTARGUMENT=&__LASTFOCUS=" +
                                 $"&__VIEWSTATE={Uri.EscapeDataString(vs)}" +
                                 $"&__VIEWSTATEGENERATOR={vsg}" +
                                 $"&__SCROLLPOSITIONX=0&__SCROLLPOSITIONY=400" +
                                 $"&__VIEWSTATEENCRYPTED=&__EVENTVALIDATION={Uri.EscapeDataString(ev)}" +
                                 $"&fin_year={Uri.EscapeDataString(fin_year)}";

                var finReq = new HttpRequestMessage(HttpMethod.Post, PRIMARY_URL)
                {
                    Content = new StringContent(finBody, Encoding.UTF8, "application/x-www-form-urlencoded")
                };
                var finResp2 = await client.SendAsync(finReq);
                string finContent = await finResp2.Content.ReadAsStringAsync();

                doc = new HtmlDocument();
                doc.LoadHtml(finContent);
                var links = doc.DocumentNode.SelectNodes("//a");
                if (links != null)
                    for (int i = 250; i < links.Count; i++)
                    {
                        var href = links[i].Attributes["href"]?.Value ?? "";
                        ftolink = "https://nregastrep.nic.in/netnrega/" + href;
                        if (href.Contains("FTO/FTOReport.aspx?"))
                            break;
                    }
            }

            if (string.IsNullOrEmpty(ftolink))
                return StatusCode(500, "FTO report link not found");

            // Step 2: GET FTOReport.aspx with session (Wage type = default, no RBtnLst POST)
            var webreq = (HttpWebRequest)WebRequest.Create(ftolink);
            webreq.Method = "GET";
            webreq.CookieContainer = new CookieContainer();
            webreq.CookieContainer.Add(new Cookie("ASP.NET_SessionId", session) { Domain = "nregastrep.nic.in" });
            var issueResp = (HttpWebResponse)await Task.Factory.FromAsync(webreq.BeginGetResponse, webreq.EndGetResponse, null);
            string blockContent = await new StreamReader(issueResp.GetResponseStream()).ReadToEndAsync();
            issueResp.Close();

            doc = new HtmlDocument();
            doc.LoadHtml(blockContent);

            // Drill down: district → block → panchayat (same XPath as original: td[2] and td[6])
            var distLinks = doc.DocumentNode.SelectNodes("//table[2]//tr//td[2]//a");
            if (distLinks == null) return StatusCode(500, "District links not found");
            string distLink = "";
            foreach (var item in distLinks)
            {
                distLink = FTO_BASE + item.Attributes["href"]?.Value;
                var qs = System.Web.HttpUtility.ParseQueryString(new Uri(distLink).Query);
                if (qs["district_code"] == district_code) break;
            }

            var reqDist = (HttpWebRequest)WebRequest.Create(distLink);
            reqDist.CookieContainer = new CookieContainer();
            reqDist.CookieContainer.Add(new Cookie("ASP.NET_SessionId", session) { Domain = "nregastrep.nic.in" });
            reqDist.Method = "GET";
            reqDist.Timeout = 10000;
            var respDist = (HttpWebResponse)await Task.Factory.FromAsync(reqDist.BeginGetResponse, reqDist.EndGetResponse, null);
            string distHtml = await new StreamReader(respDist.GetResponseStream()).ReadToEndAsync();
            respDist.Close();

            doc = new HtmlDocument();
            doc.LoadHtml(distHtml);
            var blockLinks = doc.DocumentNode.SelectNodes("//table[2]//tr//td[2]//a");
            if (blockLinks == null) return StatusCode(500, "Block links not found");
            string blockLink = "";
            foreach (var item in blockLinks)
            {
                blockLink = FTO_BASE + item.Attributes["href"]?.Value;
                var qs = System.Web.HttpUtility.ParseQueryString(new Uri(blockLink).Query);
                if (qs["block_code"] == block_code) break;
            }

            var reqBlock = (HttpWebRequest)WebRequest.Create(blockLink);
            reqBlock.CookieContainer = new CookieContainer();
            reqBlock.CookieContainer.Add(new Cookie("ASP.NET_SessionId", session) { Domain = "nregastrep.nic.in" });
            reqBlock.Method = "GET";
            reqBlock.Timeout = 10000;
            var respBlock = (HttpWebResponse)await Task.Factory.FromAsync(reqBlock.BeginGetResponse, reqBlock.EndGetResponse, null);
            string blockHtml = await new StreamReader(respBlock.GetResponseStream()).ReadToEndAsync();
            respBlock.Close();

            doc = new HtmlDocument();
            doc.LoadHtml(blockHtml);
            // Panchayat link is in td[6] (same as original)
            var panchLinks = doc.DocumentNode.SelectNodes("//table[2]//tr//td[6]//a");
            if (panchLinks == null) return StatusCode(500, "Panchayat links not found");
            string panchLink = "";
            foreach (var item in panchLinks)
            {
                panchLink = FTO_BASE + item.Attributes["href"]?.Value;
                var qs = System.Web.HttpUtility.ParseQueryString(new Uri(panchLink).Query);
                if (qs["panchayat_code"] == panchayat_code) break;
            }

            var reqPanch = (HttpWebRequest)WebRequest.Create(panchLink);
            reqPanch.Method = "GET";
            reqPanch.CookieContainer = new CookieContainer();
            reqPanch.CookieContainer.Add(new Cookie("ASP.NET_SessionId", session) { Domain = "nregastrep.nic.in" });
            var respPanch = (HttpWebResponse)await Task.Factory.FromAsync(reqPanch.BeginGetResponse, reqPanch.EndGetResponse, null);
            string finalHtml = await new StreamReader(respPanch.GetResponseStream()).ReadToEndAsync();
            respPanch.Close();

            return Content(finalHtml, "text/html");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "FtoSignDetails crawl failed");
            return StatusCode(500, "Error connecting NREGA DataBase.");
        }
    }
}
