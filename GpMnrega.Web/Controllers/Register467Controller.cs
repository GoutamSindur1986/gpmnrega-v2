using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;

namespace GpMnrega.Web.Controllers;

// ── GET /api/registers/register467 ───────────────────────────────────────────
// Mirrors register467.aspx.cs exactly.
//
// Handles registers 1, 4, 6, 7 via the ?register= query param:
//   register=4 → Work Register (quarterly, POST with quarter dates)
//   register=7 → Material Register (fin_year POST)
//   register=6 → Complaint Register (GET only)
//   register=1 → Register No. 1 Part-C (GET, optional quarter POST)
//
// All start with GET IndexFrame.aspx → extract session cookie → find specific link.
//
// Query params: register (1/4/6/7), fin_year, District_Code, district_name,
//               state_name, state_Code, block_name, block_code,
//               Panchayat_name, Panchayat_Code, quater (0-3 for 1B/4)
// ─────────────────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/registers")]
public class Register467Controller : ControllerBase
{
    private readonly ILogger<Register467Controller> _log;
    private const string NIC_BASE = "https://nregastrep.nic.in/netnrega/";

    public Register467Controller(ILogger<Register467Controller> log) => _log = log;

    [HttpGet("register467")]
    public async Task<IActionResult> Get(
        [FromQuery] int register,
        [FromQuery] string? fin_year,
        [FromQuery] string? District_Code,
        [FromQuery] string? district_name,
        [FromQuery] string? state_name,
        [FromQuery] string? state_Code,
        [FromQuery] string? block_name,
        [FromQuery] string? block_code,
        [FromQuery] string? Panchayat_name,
        [FromQuery] string? Panchayat_Code,
        [FromQuery] string? quater = "0")
    {
        try
        {
            string finyear = fin_year.Split('-')[0]; // e.g. "2025" from "2025-2026"

            // Quarter date arrays (same as original)
            string[] quartersStart = new[]
            {
                "01/04/" + finyear,
                "01/07/" + finyear,
                "01/10/" + finyear,
                "01/01/" + (int.Parse(finyear) + 1)
            };
            string[] quartersEnd = new[]
            {
                "30/06/" + finyear,
                "30/09/" + finyear,
                "31/12/" + finyear,
                "31/03/" + (int.Parse(finyear) + 1)
            };

            // Step 1: GET IndexFrame.aspx (same URL for all register types)
            string indexUrl = NIC_BASE +
                $"IndexFrame.aspx?lflag=eng&District_Code={District_Code}" +
                $"&district_name={Uri.EscapeDataString(district_name)}" +
                $"&state_name=KARNATAKA&state_Code=15" +
                $"&block_name={Uri.EscapeDataString(block_name)}&block_code={block_code}" +
                $"&fin_year={fin_year}&check=1" +
                $"&Panchayat_name={Uri.EscapeDataString(Panchayat_name)}&Panchayat_Code={Panchayat_Code}";

            using var client = new HttpClient();
            var message = await client.GetAsync(indexUrl);
            string indexContent = await message.Content.ReadAsStringAsync();

            // Extract session cookie (same as original)
            string cookies = message.Headers.GetValues("Set-Cookie").FirstOrDefault()?.Split('=')[1]?.Split(';')[0] ?? "";

            var doc1 = new HtmlDocument();
            doc1.LoadHtml(indexContent);
            var links = doc1.DocumentNode.SelectNodes("//a");
            if (links == null) return StatusCode(500, "No links in IndexFrame");

            string result = "";

            // ── Register 4: Work Register (quarterly) ────────────────────────
            if (register == 4)
            {
                string link = "";
                foreach (var item in links)
                    if (item.InnerText.Trim() == "Work Register")
                    {
                        link = NIC_BASE + Uri.UnescapeDataString(item.Attributes["href"]?.Value ?? "");
                        break;
                    }
                if (string.IsNullOrEmpty(link)) return StatusCode(500, "Work Register link not found");

                int qtr = int.Parse(quater ?? "0");
                var reg4raw = await client.GetAsync(link);
                string reg4Content = await reg4raw.Content.ReadAsStringAsync();

                var doc = new HtmlDocument { OptionUseIdAttribute = true };
                doc.LoadHtml(reg4Content);
                string vs  = doc.GetElementbyId("__VIEWSTATE")?.GetAttributeValue("value", "") ?? "";
                string vsg = doc.GetElementbyId("__VIEWSTATEGENERATOR")?.GetAttributeValue("value", "") ?? "";
                string ev  = doc.GetElementbyId("__EVENTVALIDATION")?.GetAttributeValue("value", "") ?? "";

                string body = $"__EVENTTARGET=&__EVENTARGUMENT=&__LASTFOCUS=" +
                              $"&__VIEWSTATE={Uri.EscapeDataString(vs)}" +
                              $"&__VIEWSTATEGENERATOR={Uri.EscapeDataString(vsg)}" +
                              $"&__VIEWSTATEENCRYPTED=&__EVENTVALIDATION={Uri.EscapeDataString(ev)}" +
                              $"&ctl00$ContentPlaceHolder1$TextBox3={quartersStart[qtr]}" +
                              $"&ctl00$ContentPlaceHolder1$TextBox4={quartersEnd[qtr]}" +
                              $"&ctl00$ContentPlaceHolder1$ddrows=10" +
                              $"&ctl00$ContentPlaceHolder1$Button1=submit" +
                              $"&ctl00$ContentPlaceHolder1$RadioButtonList2=0";

                var content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");
                var reg4filtered = await client.PostAsync(link, content);
                result = await reg4filtered.Content.ReadAsStringAsync();
            }

            // ── Register 7: Material Register ────────────────────────────────
            else if (register == 7)
            {
                string link = "";
                foreach (var item in links)
                    if (item.InnerText.Trim() == "Material Register")
                    {
                        link = NIC_BASE + Uri.UnescapeDataString(item.Attributes["href"]?.Value ?? "");
                        break;
                    }
                if (string.IsNullOrEmpty(link)) return StatusCode(500, "Material Register link not found");

                var reg7raw = await client.GetAsync(link);
                string reg7Content = await reg7raw.Content.ReadAsStringAsync();

                var doc = new HtmlDocument { OptionUseIdAttribute = true };
                doc.LoadHtml(reg7Content);
                string vs  = doc.GetElementbyId("__VIEWSTATE")?.GetAttributeValue("value", "") ?? "";
                string vsg = doc.GetElementbyId("__VIEWSTATEGENERATOR")?.GetAttributeValue("value", "") ?? "";
                string ev  = doc.GetElementbyId("__EVENTVALIDATION")?.GetAttributeValue("value", "") ?? "";

                string body = $"__EVENTTARGET=ctl00$ContentPlaceHolder1$ddl_finyr&__EVENTARGUMENT=&__LASTFOCUS=" +
                              $"&__VIEWSTATE={Uri.EscapeDataString(vs)}" +
                              $"&__VIEWSTATEGENERATOR={Uri.EscapeDataString(vsg)}" +
                              $"&__VIEWSTATEENCRYPTED=&__EVENTVALIDATION={Uri.EscapeDataString(ev)}" +
                              $"&ctl00$ContentPlaceHolder1$ddl_finyr={Uri.EscapeDataString(fin_year)}";

                var req = new HttpRequestMessage(HttpMethod.Post, link)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded")
                };
                var reg7filtered = await client.SendAsync(req);
                result = await reg7filtered.Content.ReadAsStringAsync();
            }

            // ── Register 6: Complaint Register (GET only) ─────────────────────
            else if (register == 6)
            {
                string link = "";
                foreach (var item in links)
                    if (item.InnerText.Trim() == "Complaint Register")
                    {
                        link = NIC_BASE + Uri.UnescapeDataString(item.Attributes["href"]?.Value ?? "");
                        break;
                    }
                if (string.IsNullOrEmpty(link)) return StatusCode(500, "Complaint Register link not found");

                var reg6raw = await client.GetAsync(link);
                result = await reg6raw.Content.ReadAsStringAsync();
            }

            // ── Register 1B: Register No. 1 Part-C ───────────────────────────
            else if (register == 1)
            {
                string link = "";
                foreach (var item in links)
                    if (item.InnerText.Trim() == "Register No. 1 Part-C")
                    {
                        link = NIC_BASE + Uri.UnescapeDataString(item.Attributes["href"]?.Value ?? "");
                        break;
                    }
                if (string.IsNullOrEmpty(link)) return StatusCode(500, "Register 1B link not found");

                var client1 = (HttpWebRequest)WebRequest.CreateHttp(link);
                client1.CookieContainer = new CookieContainer();
                client1.CookieContainer.Add(new System.Net.Cookie("ASP.NET_SessionId", cookies) { Domain = "nregastrep.nic.in" });

                var jobcardResp = (HttpWebResponse)await Task.Factory.FromAsync(client1.BeginGetResponse, client1.EndGetResponse, null);

                if (quater == "0")
                {
                    // Q1 (April-June) — return raw HTML without further POST
                    result = await new StreamReader(jobcardResp.GetResponseStream()).ReadToEndAsync();
                    jobcardResp.Close();
                }
                else
                {
                    // Other quarters need a POST to filter
                    string qtrHtml = await new StreamReader(jobcardResp.GetResponseStream()).ReadToEndAsync();
                    jobcardResp.Close();

                    // Map quater value to SQL-style quarter filter (same as original)
                    string requestQtr = quater == "1" ? "(7,8,9)"
                                      : quater == "2" ? "(10,11,12)"
                                      : "(1,2,3)";

                    var doc = new HtmlDocument { OptionUseIdAttribute = true };
                    doc.LoadHtml(qtrHtml);
                    string vs  = doc.GetElementbyId("__VIEWSTATE")?.GetAttributeValue("value", "") ?? "";
                    string vsg = doc.GetElementbyId("__VIEWSTATEGENERATOR")?.GetAttributeValue("value", "") ?? "";
                    string ev  = doc.GetElementbyId("__EVENTVALIDATION")?.GetAttributeValue("value", "") ?? "";

                    string body = $"__EVENTTARGET=ctl00$ContentPlaceHolder1$ddl_quarter&__EVENTARGUMENT=&__LASTFOCUS=" +
                                  $"&__VIEWSTATE={Uri.EscapeDataString(vs)}" +
                                  $"&__VIEWSTATEGENERATOR={Uri.EscapeDataString(vsg)}" +
                                  $"&__VIEWSTATEENCRYPTED=&__EVENTVALIDATION={Uri.EscapeDataString(ev)}" +
                                  $"&__SCROLLPOSITIONX=0&__SCROLLPOSITIONY=0" +
                                  $"&ctl00$ContentPlaceHolder1$ddl_fin={Uri.EscapeDataString(fin_year)}" +
                                  $"&ctl00$ContentPlaceHolder1$ddl_quarter={Uri.EscapeDataString(requestQtr)}" +
                                  $"&ctl00$ContentPlaceHolder1$ddrows=10";

                    var client2 = (HttpWebRequest)WebRequest.CreateHttp(link);
                    client2.CookieContainer = new CookieContainer();
                    client2.CookieContainer.Add(new System.Net.Cookie("ASP.NET_SessionId", cookies) { Domain = "nregastrep.nic.in" });
                    client2.Method = "POST";
                    byte[] bodyBytes = Encoding.UTF8.GetBytes(body);
                    client2.ContentType = "application/x-www-form-urlencoded";
                    client2.ContentLength = bodyBytes.Length;
                    using (var stream = await Task.Factory.FromAsync(client2.BeginGetRequestStream, client2.EndGetRequestStream, null))
                        await stream.WriteAsync(bodyBytes, 0, bodyBytes.Length);

                    var workResp = (HttpWebResponse)await Task.Factory.FromAsync(client2.BeginGetResponse, client2.EndGetResponse, null);
                    result = await new StreamReader(workResp.GetResponseStream()).ReadToEndAsync();
                    workResp.Close();
                }
            }
            else
            {
                return BadRequest($"Unknown register type: {register}");
            }

            return Content(result, "text/html");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Register467 crawl failed for register={R}", register);
            return StatusCode(500, "Error connecting NREGA DataBase.");
        }
    }
}
