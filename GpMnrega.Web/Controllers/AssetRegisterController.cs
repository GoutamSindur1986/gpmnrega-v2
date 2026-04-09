using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Web;

namespace GpMnrega.Web.Controllers;

// ── GET /api/registers/assetregister ─────────────────────────────────────────
// Exact async port of assetregister.aspx.cs Page_Load.
// No helper methods — logic is kept inline to match the original 1:1.
//
// Crawl flow:
//   1. GET MISreport4.aspx
//   2. POST captcha bypass (hfCaptcha value trick)
//   3. Determine currentfin from today's date
//   4a. currentfin == finyear  → POST ddl_States=15DKNY directly
//   4b. else                   → POST ddlfinyr first, then POST ddl_States=15DKNY
//   5. Find "Monitoring Report for Asset Id" link (search from index 200)
//   6. GET that link → district listing
//   7. Drill down: district_code → block_code → panchayat_code
//      (each step: prepend MIS_BASE to href, then ParseQueryString to match code)
//   8. GET panchayat asset page → return raw HTML
// ─────────────────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/registers")]
public class AssetRegisterController : ControllerBase
{
    private readonly ILogger<AssetRegisterController> _log;

    public AssetRegisterController(ILogger<AssetRegisterController> log) => _log = log;

    [HttpGet("assetregister")]
    public async Task<IActionResult> Get(
        [FromQuery] string? district_name,
        [FromQuery] string? district_code,
        [FromQuery] string? block_code,
        [FromQuery] string? block_name,
        [FromQuery] string? panchayat_code,
        [FromQuery] string? panchayat_name,
        [FromQuery] string? fin_year)
    {
        try
        {
            // local aliases — same variable names as original
            string distcode  = district_code  ?? "";
            string blockcode = block_code     ?? "";
            string pcode     = panchayat_code ?? "";
            string finyear   = fin_year       ?? "";

            string currentfin = "";
            var handler = new HttpClientHandler
            {
                CookieContainer = new System.Net.CookieContainer(),
                UseCookies = true
            };
            using var client = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(10) };
            const string UA      = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
            const string ACCEPT  = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            const string REFERER = "https://nreganarep.nic.in/netnrega/MISreport4.aspx";
            string url = "https://nreganarep.nic.in/netnrega/MISreport4.aspx";

            // Step 1: GET MISreport4.aspx
            HttpResponseMessage response = await client.GetAsync(url);
            var misresp = await response.Content.ReadAsStringAsync();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(misresp);

            // Step 2: POST login — captcha bypass (hfCaptcha == txtCaptcha)
            string captchaVal = doc.GetElementbyId("ContentPlaceHolder1_hfCaptcha")?.GetAttributeValue("value", "") ?? "";
            string __VIEWSTATE        = "__VIEWSTATE="        + Uri.EscapeDataString(doc.GetElementbyId("__VIEWSTATE")?.GetAttributeValue("value", "")        ?? "") + "&";
            string __VIEWSTATEGENERATOR = "__VIEWSTATEGENERATOR=" + (doc.GetElementbyId("__VIEWSTATEGENERATOR")?.GetAttributeValue("value", "") ?? "")               + "&";
            string __EVENTVALIDATION  = "__EVENTVALIDATION="  + Uri.EscapeDataString(doc.GetElementbyId("__EVENTVALIDATION")?.GetAttributeValue("value", "")  ?? "") + "&";
            string txtCaptcha = "ctl00$ContentPlaceHolder1$txtCaptcha=" + captchaVal + "&";
            string hfCaptcha  = "ctl00$ContentPlaceHolder1$hfCaptcha="  + captchaVal + "&";
            string btnLogin   = "ctl00$ContentPlaceHolder1$btnLogin=Verify Code";
            string body = __VIEWSTATE + __VIEWSTATEGENERATOR + __EVENTVALIDATION + txtCaptcha + hfCaptcha + btnLogin;
            HttpContent content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");
            HttpResponseMessage misresponse = await client.PostAsync(url, content);
            var result = await misresponse.Content.ReadAsStringAsync();
            doc = new HtmlDocument();
            doc.LoadHtml(result);

            var selectedOption = doc.DocumentNode.SelectSingleNode("//select[@id='ContentPlaceHolder1_ddlfinyr']//option[@selected='selected']");

            // If the attribute is just "selected" without a value (e.g., <option selected>)
            if (selectedOption == null)
            {
                selectedOption = doc.DocumentNode.SelectSingleNode("//select[@id='ContentPlaceHolder1_ddlfinyr']//option[@selected]");
            }

            string val = selectedOption?.Attributes["value"]?.Value;
            // doc.DocumentNode.SelectNodes("//a")[35].Attributes["href"].Value;
            // Step 3: yearcheck — exact logic from original
            if (DateTime.Now.Month < 3 && DateTime.Now.Month > -1)
                currentfin = (DateTime.Now.Year - 1) + "-" + DateTime.Now.Year;
            else if (DateTime.Now.Month > 2 && DateTime.Now.Month < 12)
                currentfin = DateTime.Now.Year + "-" + (DateTime.Now.Year + 1);

            // Step 4 & 5: select year/state, then find "Monitoring Report for Asset Id"
            if (val == finyear)
            {
                // POST ddl_States directly (year is already current)
                string __EVENTTARGET    = "__EVENTTARGET=ctl00$ContentPlaceHolder1$ddl_States&";
                string __EVENTARGUMENT  = "__EVENTARGUMENT=&";
                string __LASTFOCUS      = "__LASTFOCUS=&";
                __VIEWSTATE             = "__VIEWSTATE="        + Uri.EscapeDataString(doc.GetElementbyId("__VIEWSTATE")?.GetAttributeValue("value", "")        ?? "") + "&";
                __VIEWSTATEGENERATOR    = "__VIEWSTATEGENERATOR=" + (doc.GetElementbyId("__VIEWSTATEGENERATOR")?.GetAttributeValue("value", "") ?? "")               + "&";
                __EVENTVALIDATION       = "__EVENTVALIDATION="  + Uri.EscapeDataString(doc.GetElementbyId("__EVENTVALIDATION")?.GetAttributeValue("value", "")  ?? "") + "&";
                txtCaptcha              = "ctl00$ContentPlaceHolder1$txtCaptcha=&";
                hfCaptcha               = "ctl00$ContentPlaceHolder1$hfCaptcha=" + captchaVal + "&";
                string ddlfinyr1        = "ctl00$ContentPlaceHolder1$ddlfinyr=" + finyear + "&";
                string ddl_States1      = "ctl00$ContentPlaceHolder1$ddl_States=15DKNY";
                string misrespbodu1     = __EVENTTARGET + __EVENTARGUMENT + __LASTFOCUS + __VIEWSTATE + __VIEWSTATEGENERATOR + __EVENTVALIDATION + txtCaptcha + hfCaptcha + ddlfinyr1 + ddl_States1;
                HttpContent content1    = new StringContent(misrespbodu1, Encoding.UTF8, "application/x-www-form-urlencoded");
                HttpResponseMessage misre1 = await client.PostAsync(url, content1);
                var misresult1 = await misre1.Content.ReadAsStringAsync();
                doc = new HtmlDocument();
                doc.LoadHtml(misresult1);

                var requestdistlinks1 = doc.DocumentNode.SelectNodes("//a");
                string requestdistlink1 = "";
                for (int i = 200; i < requestdistlinks1.Count; i++)
                {
                    var item = requestdistlinks1[i];
                    if (item.InnerText.Trim() == "Monitoring Report for Asset Id")
                    {
                        requestdistlink1 = item.Attributes["href"]?.Value ?? ""; break;
                    }
                }
                var getReq1 = new HttpRequestMessage(HttpMethod.Get, requestdistlink1);
                getReq1.Headers.Add("User-Agent", UA);
                getReq1.Headers.Add("Referer",    REFERER);
                getReq1.Headers.Add("Accept",     ACCEPT);
                HttpResponseMessage message1 = await client.SendAsync(getReq1);
                var resmessage1 = await message1.Content.ReadAsStringAsync();
                doc = new HtmlDocument();
                doc.LoadHtml(resmessage1);
            }
            else
            {
                // POST ddlfinyr first to change the year
                string __EVENTTARGET2   = "__EVENTTARGET=ctl00$ContentPlaceHolder1$ddlfinyr&";
                string __EVENTARGUMENT2 = "__EVENTARGUMENT=&";
                string __LASTFOCUS2     = "__LASTFOCUS=&";
                __VIEWSTATE             = "__VIEWSTATE="        + Uri.EscapeDataString(doc.GetElementbyId("__VIEWSTATE")?.GetAttributeValue("value", "")        ?? "") + "&";
                __VIEWSTATEGENERATOR    = "__VIEWSTATEGENERATOR=" + (doc.GetElementbyId("__VIEWSTATEGENERATOR")?.GetAttributeValue("value", "") ?? "")               + "&";
                __EVENTVALIDATION       = "__EVENTVALIDATION="  + Uri.EscapeDataString(doc.GetElementbyId("__EVENTVALIDATION")?.GetAttributeValue("value", "")  ?? "") + "&";
                // original re-reads hfCaptcha from post-login doc (same element, possibly refreshed)
                string captchaVal2      = doc.GetElementbyId("ContentPlaceHolder1_hfCaptcha")?.GetAttributeValue("value", "") ?? "";
                txtCaptcha              = "ctl00$ContentPlaceHolder1$txtCaptcha=" + captchaVal2 + "&";
                hfCaptcha               = "ctl00$ContentPlaceHolder1$hfCaptcha="  + captchaVal2 + "&";
                string ddlfinyr2        = "ctl00$ContentPlaceHolder1$ddlfinyr=" + finyear + "&";
                string ddl_States2      = "ctl00$ContentPlaceHolder1$ddl_States=99";
                // NOTE: original postbody omits hfCaptcha — exact match
                string postbody         = __EVENTTARGET2 + __EVENTARGUMENT2 + __LASTFOCUS2 + __VIEWSTATE + __VIEWSTATEGENERATOR + __EVENTVALIDATION + txtCaptcha + ddlfinyr2 + ddl_States2;
                HttpContent content2    = new StringContent(postbody, Encoding.UTF8, "application/x-www-form-urlencoded");
                HttpResponseMessage misre2 = await client.PostAsync(url, content2);
                var misresult2 = await misre2.Content.ReadAsStringAsync();
                doc = new HtmlDocument();
                doc.LoadHtml(misresult2);

                // Then POST ddl_States = Karnataka (15DKNY)
                string __EVENTTARGET3   = "__EVENTTARGET=ctl00$ContentPlaceHolder1$ddl_States&";
                string __EVENTARGUMENT3 = "__EVENTARGUMENT=&";
                string __LASTFOCUS3     = "__LASTFOCUS=&";
                __VIEWSTATE             = "__VIEWSTATE="        + Uri.EscapeDataString(doc.GetElementbyId("__VIEWSTATE")?.GetAttributeValue("value", "")        ?? "") + "&";
                __VIEWSTATEGENERATOR    = "__VIEWSTATEGENERATOR=" + (doc.GetElementbyId("__VIEWSTATEGENERATOR")?.GetAttributeValue("value", "") ?? "")               + "&";
                __EVENTVALIDATION       = "__EVENTVALIDATION="  + Uri.EscapeDataString(doc.GetElementbyId("__EVENTVALIDATION")?.GetAttributeValue("value", "")  ?? "") + "&";
                txtCaptcha              = "ctl00$ContentPlaceHolder1$txtCaptcha=&";
                hfCaptcha               = "ctl00$ContentPlaceHolder1$hfCaptcha=&";   // empty — matches original line 150
                string ddlfinyr3        = "ctl00$ContentPlaceHolder1$ddlfinyr=" + finyear + "&";
                string ddl_States3      = "ctl00$ContentPlaceHolder1$ddl_States=15DKNY";
                string misrespbodu3     = __EVENTTARGET3 + __EVENTARGUMENT3 + __LASTFOCUS3 + __VIEWSTATE + __VIEWSTATEGENERATOR + __EVENTVALIDATION + txtCaptcha + hfCaptcha + ddlfinyr3 + ddl_States3;
                HttpContent stateresp   = new StringContent(misrespbodu3, Encoding.UTF8, "application/x-www-form-urlencoded");
                HttpResponseMessage misrep = await client.PostAsync(url, stateresp);
                var misresu = await misrep.Content.ReadAsStringAsync();
                doc = new HtmlDocument();
                doc.LoadHtml(misresu);

                var requestdistlinks3 = doc.DocumentNode.SelectNodes("//a");
                string requestdistlink3 = "";
                for (int i = 200; i < requestdistlinks3.Count; i++)
                {
                    var item = requestdistlinks3[i];
                    if (item.InnerText.Trim() == "Monitoring Report for Asset Id")
                    {
                        requestdistlink3 = item.Attributes["href"]?.Value ?? ""; break;
                    }
                }
                var getReq3 = new HttpRequestMessage(HttpMethod.Get, requestdistlink3);
                getReq3.Headers.Add("User-Agent", UA);
                getReq3.Headers.Add("Referer",    REFERER);
                getReq3.Headers.Add("Accept",     ACCEPT);
                HttpResponseMessage message3 = await client.SendAsync(getReq3);
                var resmessage3 = await message3.Content.ReadAsStringAsync();
                doc = new HtmlDocument();
                doc.LoadHtml(resmessage3);
            }

            // Step 7a: Find district link — exact port of original loop
            var distlinks = doc.DocumentNode.SelectNodes("//a");
            string finaldistlink = "";
            if (distlinks != null)
            {
                foreach (var link in distlinks)
                {
                    var reqdistlink = "https://mnregaweb4.nic.in/netnrega/" + (link.Attributes["href"]?.Value ?? "");
                    var parser = HttpUtility.ParseQueryString(reqdistlink);
                    if (parser != null && parser.Get("district_code") == distcode)
                    {
                        finaldistlink = reqdistlink; break;
                    }
                }
            }
            if (string.IsNullOrEmpty(finaldistlink))
                return StatusCode(500, "District link not found");

            // Step 7b: Find block link
            var distReq = new HttpRequestMessage(HttpMethod.Get, finaldistlink);
            distReq.Headers.Add("User-Agent", UA);
            distReq.Headers.Add("Referer",    REFERER);
            distReq.Headers.Add("Accept",     ACCEPT);
            HttpResponseMessage distresp = await client.SendAsync(distReq);
            var blockresp = await distresp.Content.ReadAsStringAsync();
            doc = new HtmlDocument();
            doc.LoadHtml(blockresp);
            var blinks = doc.DocumentNode.SelectNodes("//a");
            string finalblink = "";
            if (blinks != null)
            {
                foreach (var blink in blinks)
                {
                    string requestblink = "https://mnregaweb4.nic.in/netnrega/" + (blink.Attributes["href"]?.Value ?? "");
                    var parser = HttpUtility.ParseQueryString(requestblink);
                    if (parser != null && parser.Get("block_code") == blockcode)
                    {
                        finalblink = requestblink; break;
                    }
                }
            }
            if (string.IsNullOrEmpty(finalblink))
                return StatusCode(500, "Block link not found");

            // Step 7c: Find panchayat link
            var blockReq = new HttpRequestMessage(HttpMethod.Get, finalblink);
            blockReq.Headers.Add("User-Agent", UA);
            blockReq.Headers.Add("Referer",    REFERER);
            blockReq.Headers.Add("Accept",     ACCEPT);
            HttpResponseMessage bresp = await client.SendAsync(blockReq);
            var plinkHtml = await bresp.Content.ReadAsStringAsync();
            doc = new HtmlDocument();
            doc.LoadHtml(plinkHtml);
            var plink = doc.DocumentNode.SelectNodes("//a");
            string finplink = "";
            if (plink != null)
            {
                foreach (var link in plink)
                {
                    string reqplink = "https://mnregaweb4.nic.in/netnrega/" + (link.Attributes["href"]?.Value ?? "");
                    var parser = HttpUtility.ParseQueryString(reqplink);
                    if (parser != null && parser.Get("panchayat_code") == pcode)
                    {
                        finplink = reqplink; break;
                    }
                }
            }
            if (string.IsNullOrEmpty(finplink))
                return StatusCode(500, "Panchayat link not found");

            // Step 8: GET panchayat asset page → return raw HTML
            var panchReq = new HttpRequestMessage(HttpMethod.Get, finplink);
            panchReq.Headers.Add("User-Agent", UA);
            panchReq.Headers.Add("Referer",    REFERER);
            panchReq.Headers.Add("Accept",     ACCEPT);
            HttpResponseMessage finalpanresp = await client.SendAsync(panchReq);
            var finalresp = await finalpanresp.Content.ReadAsStringAsync();

            return Content(finalresp, "text/html");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "AssetRegister crawl failed");
            return StatusCode(500, "Error connecting NREGA DataBase.");
        }
    }
}
