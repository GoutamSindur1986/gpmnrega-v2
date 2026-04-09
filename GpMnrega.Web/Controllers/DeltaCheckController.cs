using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;
using System.Web;

namespace GpMnrega.Web.Controllers;

// ── GET /api/proxy/deltacheck ─────────────────────────────────────────────────
// Mirrors deltacheck.aspx.cs exactly.
//
// Purpose: find NMRs that have been ISSUED on NIC but are not yet in our datasync
//          DB snapshot (i.e. the "issued delta").  The caller (gphome.js checkdelta)
//          sends the list of already-known NMR numbers in the POST body as a
//          comma-separated string and passes GP/work identifiers in query params.
//
// Crawl steps (matching the original):
//   1. GET  IndexFrame.aspx (GP summary page) → find "Muster Roll" link
//   2. GET  Muster Roll listing page           → ViewState
//   3. POST btnprin (sync)                     → issued NMR listing → ViewState
//   4. POST ddlwork=work_code (async)          → NMR dropdown for this work
//   5. For each option in ddlMsrno NOT in loaded list:
//        POST ddlMsrno=<value> (async)         → NMR detail HTML (UpdatePanel2)
//        Parse HTML → extract NMR#, dates, JC table
//   6. Return JSON { newNmrs: [...] }
//
// Returns: { newNmrs: [ { nmrNo, fromDate, toDate, nmrLink, status, jc:[...] } ] }
// ─────────────────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/proxy")]
public class DeltaCheckController : ControllerBase
{
    private readonly ILogger<DeltaCheckController> _log;
    private const string UA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36";

    public DeltaCheckController(ILogger<DeltaCheckController> log) => _log = log;

    // ── HttpClient factory: same cookie jar per crawl session ─────────────
    private static HttpClient CreateNicClient()
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect    = true,
            UseCookies           = true,
            CookieContainer      = new CookieContainer(),
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(120) };
        client.DefaultRequestHeaders.Add("User-Agent", UA);
        return client;
    }

    [HttpPost("deltacheck")]
    public async Task<IActionResult> DeltaCheck(
        [FromQuery] string  work_code,
        [FromQuery] string  dist_code,
        [FromQuery] string  block_code,
        [FromQuery] string  panch_code,
        [FromQuery] string  dist_name,
        [FromQuery] string  block_name,
        [FromQuery] string  panch_name,
        [FromQuery] string  fin_year    = "2025-2026",
        [FromQuery] string  state_code  = "15",
        [FromQuery] string  state_name  = "KARNATAKA")
    {
        // Read already-loaded NMR numbers from POST body (comma-separated)
        string bodyRaw;
        using (var sr = new StreamReader(Request.Body))
            bodyRaw = await sr.ReadToEndAsync();

        var nmrLoaded = bodyRaw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        try
        {
            using var c = CreateNicClient();

            // ── Step 1: GET IndexFrame → find "Muster Roll" link ─────────────
            var indexUrl = "https://nregastrep.nic.in/netnrega/IndexFrame.aspx" +
                           $"?lflag=eng&District_Code={dist_code}&district_name={Uri.EscapeDataString(dist_name)}" +
                           $"&state_name={Uri.EscapeDataString(state_name)}&state_Code={state_code}" +
                           $"&block_name={Uri.EscapeDataString(block_name)}&block_code={block_code}" +
                           $"&fin_year={Uri.EscapeDataString(fin_year)}&check=1" +
                           $"&Panchayat_name={Uri.EscapeDataString(panch_name)}&Panchayat_Code={panch_code}";

            var indexHtml = await GetAsync(c, indexUrl);
            var indexDoc  = LoadHtml(indexHtml);

            // Find the "Muster Roll" anchor (matches original loop)
            var mustLink = "";
            var anchors  = indexDoc.DocumentNode.SelectNodes("//a");
            if (anchors != null)
                foreach (var a in anchors)
                    if (a.InnerText.Trim().Equals("Muster Roll", StringComparison.OrdinalIgnoreCase))
                    {
                        mustLink = a.GetAttributeValue("href", "");
                        break;
                    }

            if (string.IsNullOrWhiteSpace(mustLink))
            {
                _log.LogWarning("deltacheck: 'Muster Roll' link not found for panch={Panch}", panch_code);
                return Ok(new { newNmrs = Array.Empty<object>() });
            }

            var mustUrl = "https://nregastrep.nic.in/netnrega/" + mustLink;

            // ── Step 2: GET Muster Roll listing page → ViewState ──────────────
            var mustHtml = await GetAsync(c, mustUrl);
            var mustDoc  = LoadHtml(mustHtml);
            string Vs(HtmlDocument d, string id) =>
                d.GetElementbyId(id)?.GetAttributeValue("value", "") ?? "";

            // ── Step 3: POST btnprin (sync) → issued NMR listing ──────────────
            // Mirrors the original sync POST that clicks the "Print" / load button.
            var p3 = new Dictionary<string, string>
            {
                ["__EVENTTARGET"]        = "ctl00$ContentPlaceHolder1$btnprin",
                ["__EVENTARGUMENT"]      = "",
                ["__LASTFOCUS"]          = "",
                ["__VIEWSTATE"]          = Vs(mustDoc, "__VIEWSTATE"),
                ["__VIEWSTATEGENERATOR"] = Vs(mustDoc, "__VIEWSTATEGENERATOR"),
                ["__VIEWSTATEENCRYPTED"] = "",
                ["__EVENTVALIDATION"]    = Vs(mustDoc, "__EVENTVALIDATION"),
                ["__SCROLLPOSITIONX"]    = "0",
                ["__SCROLLPOSITIONY"]    = "0",
                ["ctl00$ContentPlaceHolder1$ddlFinYear"] = fin_year,
                ["ctl00$ContentPlaceHolder1$btnfill"]    = "btnfill",
                ["ctl00$ContentPlaceHolder1$btnprin"]    = "btnprin",
                ["ctl00$ContentPlaceHolder1$txtSearch"]  = "",
                ["ctl00$ContentPlaceHolder1$ddlwork"]    = "---select---",
                ["ctl00$ContentPlaceHolder1$ddlMsrno"]   = "---select---"
            };
            var r3html = await PostAsync(c, mustUrl, p3);
            var r3doc  = LoadHtml(r3html);

            // ── Step 4: POST ddlwork=work_code (async UpdatePanel) ────────────
            // Mirrors original postdata1 with ScriptManager + __ASYNCPOST
            var p4 = new Dictionary<string, string>
            {
                ["__ASYNCPOST"]          = "true",
                ["__EVENTTARGET"]        = "ctl00$ContentPlaceHolder1$ddlwork",
                ["__EVENTARGUMENT"]      = "",
                ["__LASTFOCUS"]          = "",
                ["__VIEWSTATE"]          = Vs(r3doc, "__VIEWSTATE"),
                ["__VIEWSTATEGENERATOR"] = Vs(r3doc, "__VIEWSTATEGENERATOR"),
                ["__VIEWSTATEENCRYPTED"] = Vs(r3doc, "__VIEWSTATEENCRYPTED"),
                ["__EVENTVALIDATION"]    = Vs(r3doc, "__EVENTVALIDATION"),
                ["__SCROLLPOSITIONX"]    = "0",
                ["__SCROLLPOSITIONY"]    = "0",
                ["ctl00$ContentPlaceHolder1$ScriptManager1"] =
                    "ctl00$ContentPlaceHolder1$ScriptManager1|ctl00$ContentPlaceHolder1$ddlwork",
                ["ctl00$ContentPlaceHolder1$ddlFinYear"] = fin_year,
                ["ctl00$ContentPlaceHolder1$btnprin"]    = "btnprin",
                ["ctl00$ContentPlaceHolder1$ddlwork"]    = work_code.ToUpper(),
                ["ctl00$ContentPlaceHolder1$txtSearch"]  = work_code.ToUpper(),
                ["ctl00$ContentPlaceHolder1$ddlMsrno"]   = "---Select---"
            };
            var r4raw  = await PostRawAsync(c, mustUrl, p4);
            var r4parts = r4raw.Split('|');
            string TokRaw(string[] parts, string key)
            {
                var idx = Array.IndexOf(parts, key);
                return idx >= 0 && idx + 1 < parts.Length ? parts[idx + 1] : "";
            }

            // Parse ddlMsrno dropdown from the async partial HTML
            // The UpdatePanel2 chunk contains the refreshed dropdown HTML
            var r4doc = LoadHtml(r4raw);
            var ddlNode = r4doc.GetElementbyId("ctl00_ContentPlaceHolder1_ddlMsrno");
            var options = ddlNode?.SelectNodes("option");

            if (options == null || options.Count <= 1)
            {
                _log.LogInformation("deltacheck: no NMR options for work={Work}", work_code);
                return Ok(new { newNmrs = Array.Empty<object>() });
            }

            // ── Step 5: For each NMR option not in loaded list ────────────────
            // option InnerText format: "874~01/04/2024~15/04/2024" (NMR~from~to)
            // Mirrors: msrno[i].InnerText.Split('~')[0].Trim()
            var newNmrs = new List<object>();

            // Keep track of ViewState across sequential async posts (UpdatePanel)
            var vsState  = TokRaw(r4parts, "__VIEWSTATE");
            var vsGen    = TokRaw(r4parts, "__VIEWSTATEGENERATOR");
            var vsEnc    = TokRaw(r4parts, "__VIEWSTATEENCRYPTED");
            var vsVal    = TokRaw(r4parts, "__EVENTVALIDATION");

            // If split didn't find tokens, fall back to what r3doc had
            if (string.IsNullOrEmpty(vsState)) vsState = Vs(r3doc, "__VIEWSTATE");
            if (string.IsNullOrEmpty(vsGen))   vsGen   = Vs(r3doc, "__VIEWSTATEGENERATOR");
            if (string.IsNullOrEmpty(vsVal))   vsVal   = Vs(r3doc, "__EVENTVALIDATION");

            for (int i = 1; i < options.Count; i++) // skip index 0 (---Select---)
            {
                try
                {
                    var optValue = options[i].GetAttributeValue("value", "").Trim();
                    var optText  = options[i].InnerText.Trim();
                    var nmrNo    = optText.Split('~')[0].Trim();

                    // Skip NMRs already in our DB snapshot
                    if (nmrLoaded.Contains(nmrNo)) continue;

                    // POST async ddlMsrno=<value> to get this NMR's detail page
                    var p5 = new Dictionary<string, string>
                    {
                        ["__ASYNCPOST"]          = "true",
                        ["__EVENTTARGET"]        = "ctl00$ContentPlaceHolder1$ddlMsrno",
                        ["__EVENTARGUMENT"]      = "",
                        ["__LASTFOCUS"]          = "",
                        ["__VIEWSTATE"]          = vsState,
                        ["__VIEWSTATEGENERATOR"] = vsGen,
                        ["__VIEWSTATEENCRYPTED"] = vsEnc,
                        ["__EVENTVALIDATION"]    = vsVal,
                        ["ctl00$ContentPlaceHolder1$ScriptManager1"] =
                            "ctl00$ContentPlaceHolder1$UpdatePanel2|ctl00$ContentPlaceHolder1$ddlMsrno",
                        ["ctl00$ContentPlaceHolder1$ddlFinYear"] = fin_year,
                        ["ctl00$ContentPlaceHolder1$btnprin"]    = "btnprin",
                        ["ctl00$ContentPlaceHolder1$ddlwork"]    = work_code.ToUpper(),
                        ["ctl00$ContentPlaceHolder1$txtSearch"]  = work_code.ToUpper(),
                        ["ctl00$ContentPlaceHolder1$ddlMsrno"]   = optValue
                    };
                    var r5raw   = await PostRawAsync(c, mustUrl, p5);
                    var r5parts = r5raw.Split('|');

                    // Update ViewState for next iteration (mirrors original sequential behaviour)
                    var nextVs  = TokRaw(r5parts, "__VIEWSTATE");
                    var nextGen = TokRaw(r5parts, "__VIEWSTATEGENERATOR");
                    var nextEnc = TokRaw(r5parts, "__VIEWSTATEENCRYPTED");
                    var nextVal = TokRaw(r5parts, "__EVENTVALIDATION");
                    if (!string.IsNullOrEmpty(nextVs))  vsState = nextVs;
                    if (!string.IsNullOrEmpty(nextGen)) vsGen   = nextGen;
                    if (!string.IsNullOrEmpty(nextEnc)) vsEnc   = nextEnc;
                    if (!string.IsNullOrEmpty(nextVal)) vsVal   = nextVal;

                    // Parse the UpdatePanel2 chunk to extract NMR detail
                    // Original split: data.split('UpdatePanel2|\r\n')
                    var nmrEntry = ParseNmrDetail(r5raw, mustUrl, nmrNo);
                    if (nmrEntry != null)
                        newNmrs.Add(nmrEntry);
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "deltacheck: error processing option {Idx} for work={Work}", i, work_code);
                }
            }

            return Ok(new { newNmrs });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "deltacheck failed for work={Work}", work_code);
            return StatusCode(502, new { error = "Delta check failed." });
        }
    }

    // ── Parse one UpdatePanel2 async response chunk ───────────────────────────
    // Mirrors the original checkdelta loop body in gphome.js:
    //   msrno  ← #ctl00_ContentPlaceHolder1_lblMsrNo2
    //   from   ← #ctl00_ContentPlaceHolder1_lbldatefrom
    //   to     ← #ctl00_ContentPlaceHolder1_lbldateto
    //   JC     ← table#ctl00_ContentPlaceHolder1_grdShowRecords rows
    private static object? ParseNmrDetail(string raw, string mustUrl, string fallbackNmrNo)
    {
        try
        {
            // The async response is a pipe-delimited UpdatePanel blob.
            // Split on 'UpdatePanel2|' to reach the HTML chunk (same as original).
            const string marker = "UpdatePanel2|\r\n";
            var chunkStart = raw.IndexOf(marker, StringComparison.Ordinal);
            var html = chunkStart >= 0 ? raw[(chunkStart + marker.Length)..] : raw;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            string GetInner(string id) =>
                doc.GetElementbyId(id)?.InnerText.Trim() ?? "";

            var nmrNo    = GetInner("ctl00_ContentPlaceHolder1_lblMsrNo2");
            if (string.IsNullOrEmpty(nmrNo)) nmrNo = fallbackNmrNo;

            var fromDate = GetInner("ctl00_ContentPlaceHolder1_lbldatefrom");
            var toDate   = GetInner("ctl00_ContentPlaceHolder1_lbldateto");

            // Build Musternew URL (same approach as ParseNmrLinks)
            var nmrLink = "https://nregastrep.nic.in/netnrega/citizen_html/Musternew.aspx" +
                          $"?state_code=15&state_name=KARNATAKA&msrno={Uri.EscapeDataString(nmrNo)}";

            // ── JC table (mirrors original checkdelta JC extraction) ──────────
            var jcList  = new List<object>();
            var tableId = "ctl00_ContentPlaceHolder1_grdShowRecords";
            var tableNode = doc.GetElementbyId(tableId);
            if (tableNode != null)
            {
                var rows = tableNode.SelectNodes(".//tr");
                if (rows != null && rows.Count > 1)
                {
                    // Determine dynamic column indices from header row (same as original)
                    var headerCells = rows[0].SelectNodes("th|td");
                    var colCount    = headerCells?.Count ?? 0;
                    var wageListId  = colCount > 5  ? colCount - 5 : 0;
                    var bankNameId  = colCount > 8  ? colCount - 8 : 0;
                    var dateCreId   = colCount > 3  ? colCount - 3 : 0;

                    for (int j = 1; j < rows.Count - 1; j++)
                    {
                        var cells = rows[j].SelectNodes("td");
                        if (cells == null || cells.Count < 4) continue;

                        var jcAnchor  = cells[1].SelectSingleNode(".//a");
                        var jcNo      = jcAnchor?.InnerText.Trim() ?? "";

                        // applicantName: first <br> segment, strip parenthetical (mirrors original)
                        var rawName    = cells[1].InnerHtml.Split("<br>", StringSplitOptions.None)[0];
                        var appName    = StripHtml(rawName);
                        var parenIdx   = appName.IndexOf('(');
                        if (parenIdx > 0) appName = appName[..parenIdx].Trim();

                        var category  = cells[2].InnerText.Trim();
                        if (category.Length > 3) category = category[..3];

                        jcList.Add(new
                        {
                            jcNo,
                            appName,
                            appAddress  = cells.Count > 3 ? cells[3].InnerText.Trim() : "",
                            category,
                            wageList    = wageListId  > 0 && cells.Count > wageListId  ? cells[wageListId].InnerText.Trim()  : "",
                            bankName    = bankNameId  > 0 && cells.Count > bankNameId  ? cells[bankNameId].InnerText.Trim()  : "",
                            dateCreated = dateCreId   > 0 && cells.Count > dateCreId   ? cells[dateCreId].InnerText.Trim()   : ""
                        });
                    }
                }
            }

            return new
            {
                nmrNo,
                fromDate,
                toDate,
                nmrLink,
                status  = "Issued",
                jc      = jcList
            };
        }
        catch
        {
            return null;
        }
    }

    // ── HTTP helpers ──────────────────────────────────────────────────────────
    private static async Task<string> GetAsync(HttpClient c, string url)
    {
        using var resp = await c.GetAsync(url);
        return await resp.Content.ReadAsStringAsync();
    }

    private static async Task<string> PostAsync(HttpClient c, string url, Dictionary<string, string> form)
    {
        using var resp = await c.PostAsync(url, new FormUrlEncodedContent(form));
        return await resp.Content.ReadAsStringAsync();
    }

    // Raw POST — returns the full text including pipe-delimited UpdatePanel tokens
    private static async Task<string> PostRawAsync(HttpClient c, string url, Dictionary<string, string> form)
    {
        using var resp = await c.PostAsync(url, new FormUrlEncodedContent(form));
        var bytes = await resp.Content.ReadAsByteArrayAsync();
        return Encoding.UTF8.GetString(bytes);
    }

    private static HtmlDocument LoadHtml(string html)
    {
        var doc = new HtmlDocument { OptionUseIdAttribute = true };
        doc.LoadHtml(html);
        return doc;
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return "";
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return doc.DocumentNode.InnerText;
    }
}
