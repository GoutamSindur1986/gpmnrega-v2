using GpMnrega.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HtmlAgilityPack;
using EvoPdf;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Web;

namespace GpMnrega.Web.Controllers;

/// <summary>
/// Replaces all /api/*.aspx pages.
/// WASM and jQuery both call these endpoints.
/// This server proxies to NIC — no CORS issues ever.
/// All endpoints require authentication.
/// </summary>
[Authorize]
[Route("api/proxy")]
[ApiController]
public class ProxyController : ControllerBase
{
    private readonly INicProxyService _nic;
    private readonly ILogger<ProxyController> _log;

    public ProxyController(INicProxyService nic, ILogger<ProxyController> log)
    {
        _nic = nic;
        _log = log;
    }

    // ── Helper: create HttpClient with User-Agent for NIC crawl ──────
    // NIC servers are slow — use 60s per-call timeout.
    // Set UseCookies=false when no container supplied so manual Cookie headers work.
    private static HttpClient CreateNicClient(CookieContainer? cookies = null)
    {
        var handler = new HttpClientHandler();
        if (cookies != null)
        {
            handler.CookieContainer = cookies;
            handler.UseCookies = true;
        }
        else
        {
            handler.UseCookies = false;   // allow manual Cookie header
        }
        var client = new HttpClient(handler);
        client.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");
        client.Timeout = TimeSpan.FromSeconds(60);
        return client;
    }

    // ── Helper: retry a NIC GET up to 2 times on transient failure ───
    private static async Task<string> NicGetWithRetryAsync(HttpClient client, string url, int retries = 2)
    {
        for (int attempt = 0; attempt <= retries; attempt++)
        {
            try
            {
                var resp = await client.GetAsync(url);
                return await resp.Content.ReadAsStringAsync();
            }
            catch (Exception ex) when (attempt < retries &&
                (ex is HttpRequestException || ex is TaskCanceledException || ex is IOException))
            {
                await Task.Delay(1000 * (attempt + 1));  // 1s, 2s back-off
            }
        }
        return "";
    }

    // ── GET /api/proxy/getworkdata ─────────────────────────────────
    // Replaces: /api/getworkdata.aspx
    // Multi-step NIC crawl: state → district → block → GP → work list
    // Returns JSON dictionary: { "WORK_CODE": ["name", "status", "link", "code"], ... }
    [HttpGet("getworkdata")]
    public async Task<IActionResult> GetWorkData(
        [FromQuery] string? district_code,
        [FromQuery] string? block_code,
        [FromQuery] string? panchayat_code,
        [FromQuery] string fin_year = "2025-2026",
        [FromQuery] string? state_name = "KARNATAKA",
        [FromQuery] string? state_code = "15",
        [FromQuery] string? district_name = "",
        [FromQuery] string? block_name = "",
        [FromQuery] string? panchayat_name = "",
        [FromQuery] string? short_name = "KN",
        [FromQuery] string? work_name = "",
        [FromQuery] string? source = "",
        [FromQuery] string? Digest = "s3kn")
    {
        try
        {
            var baseUrl = "https://nregastrep.nic.in/netnrega/";
            using var step1Client = CreateNicClient();

            // Step 1: Get state home page → find "DPR Frozen Status" link
            var homeUrl = baseUrl + "homestciti.aspx?state_code=15&state_name=KARNATAKA&lflag=eng&labels=labels";
            _log.LogInformation("getworkdata: Step 1 — fetching home page {Url}", homeUrl);
            var homeHtml = await NicGetWithRetryAsync(step1Client, homeUrl);

            // Fallback if NIC primary server is down or returns tampered message
            if (string.IsNullOrWhiteSpace(homeHtml) || homeHtml.Contains("URL TEMPERED"))
            {
                _log.LogWarning("getworkdata: Primary NIC down or tampered, trying fallback DPR URL directly");
                baseUrl = "https://mnregaweb4.nic.in/netnrega/";
                homeUrl = baseUrl + "state_html/wrk_cat_freeze.aspx?page=S&short_name=KN&state_name=KARNATAKA&state_code=15&fin_year=" + fin_year + "&source=national&Digest=7LVcX/pXCsHpVKoF+Bjpcg";
                homeHtml = await NicGetWithRetryAsync(step1Client, homeUrl);
            }

            if (string.IsNullOrWhiteSpace(homeHtml))
            {
                _log.LogWarning("getworkdata: Empty response from NIC home page");
                return StatusCode(502, new { error = "NIC server did not respond. Please try again." });
            }

            // Helper: resolve a possibly-relative href against a page URL
            static string ResolveUrl(string pageUrl, string href)
            {
                if (string.IsNullOrEmpty(href)) return "";
                if (href.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    href.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    return href;
                try { return new Uri(new Uri(pageUrl), href).ToString(); }
                catch { return ""; }
            }

            // Helper: find a link whose query string has a given param=value
            static string FindLinkByQueryParam(HtmlNodeCollection? links, string pageUrl, string paramName, string? paramValue)
            {
                if (links == null || string.IsNullOrEmpty(paramValue)) return "";
                foreach (var link in links)
                {
                    var href = link.Attributes["href"]?.Value ?? "";
                    if (string.IsNullOrEmpty(href)) continue;
                    var resolved = ResolveUrl(pageUrl, href);
                    if (string.IsNullOrEmpty(resolved)) continue;
                    try
                    {
                        var parms = HttpUtility.ParseQueryString(new Uri(resolved).Query);
                        if (string.Equals(parms.Get(paramName), paramValue, StringComparison.OrdinalIgnoreCase))
                            return resolved;
                    }
                    catch { }
                }
                return "";
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(homeHtml);
            var homeLinks = doc.DocumentNode.SelectNodes("//a");
            string dprUrl = "";
            if (homeLinks != null)
            {
                // Search for "DPR Frozen Status" link — case-insensitive, trimmed
                // Try from index 300 first (skip nav), then full scan
                int startIdx = Math.Min(200, homeLinks.Count - 1);
                for (int pass = 0; pass < 2 && string.IsNullOrEmpty(dprUrl); pass++)
                {
                    int from = pass == 0 ? startIdx : 0;
                    int to   = pass == 0 ? homeLinks.Count : startIdx;
                    for (int i = from; i < to; i++)
                    {
                        var text = homeLinks[i].InnerText.Trim();
                        if (text.IndexOf("DPR Frozen", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            var href = homeLinks[i].Attributes["href"]?.Value ?? "";
                            dprUrl = ResolveUrl(homeUrl, href);
                            _log.LogInformation("getworkdata: Found DPR link text='{Text}' url={Url}", text, dprUrl);
                            break;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(dprUrl))
            {
                _log.LogWarning("getworkdata: DPR Frozen Status link not found. Home page length={Len}", homeHtml.Length);
                return Ok(new Dictionary<string, string[]>());
            }

            // Step 2: Navigate DPR state page → find district link
            // Cookie-aware client so NIC session state carries across steps 2-5
            _log.LogInformation("getworkdata: Step 2 — fetching DPR state page {Url}", dprUrl);
            var crawlCookies = new CookieContainer();
            using var crawlClient = CreateNicClient(crawlCookies);

            var dprHtml = await NicGetWithRetryAsync(crawlClient, dprUrl);
            if (string.IsNullOrWhiteSpace(dprHtml))
            {
                _log.LogWarning("getworkdata: Empty DPR state page response");
                return Ok(new Dictionary<string, string[]>());
            }
            doc = new HtmlDocument();
            doc.LoadHtml(dprHtml);
            string distUrl = FindLinkByQueryParam(doc.DocumentNode.SelectNodes("//a"), dprUrl, "district_code", district_code);

            if (string.IsNullOrEmpty(distUrl))
            {
                _log.LogWarning("getworkdata: District {Dist} not found in DPR page (links={Count})", district_code, doc.DocumentNode.SelectNodes("//a")?.Count ?? 0);
                return Ok(new Dictionary<string, string[]>());
            }

            // Step 3: District page → find block link
            _log.LogInformation("getworkdata: Step 3 — fetching district page {Url}", distUrl);
            var distHtml = await NicGetWithRetryAsync(crawlClient, distUrl);
            if (string.IsNullOrWhiteSpace(distHtml))
            {
                _log.LogWarning("getworkdata: Empty district page response");
                return Ok(new Dictionary<string, string[]>());
            }
            doc = new HtmlDocument();
            doc.LoadHtml(distHtml);
            string blockUrl = FindLinkByQueryParam(doc.DocumentNode.SelectNodes("//a"), distUrl, "block_code", block_code);

            if (string.IsNullOrEmpty(blockUrl))
            {
                _log.LogWarning("getworkdata: Block {Block} not found in district page (links={Count})", block_code, doc.DocumentNode.SelectNodes("//a")?.Count ?? 0);
                return Ok(new Dictionary<string, string[]>());
            }

            // Step 4: Block page → find GP link
            _log.LogInformation("getworkdata: Step 4 — fetching block page {Url}", blockUrl);
            var blockHtml = await NicGetWithRetryAsync(crawlClient, blockUrl);
            if (string.IsNullOrWhiteSpace(blockHtml))
            {
                _log.LogWarning("getworkdata: Empty block page response");
                return Ok(new Dictionary<string, string[]>());
            }
            doc = new HtmlDocument();
            doc.LoadHtml(blockHtml);
            string gpUrl = FindLinkByQueryParam(doc.DocumentNode.SelectNodes("//a"), blockUrl, "panchayat_code", panchayat_code);

            if (string.IsNullOrEmpty(gpUrl))
            {
                _log.LogWarning("getworkdata: GP {GP} not found in block page (links={Count})", panchayat_code, doc.DocumentNode.SelectNodes("//a")?.Count ?? 0);
                return Ok(new Dictionary<string, string[]>());
            }

            // Step 5: GP page → parse work table rows
            _log.LogInformation("getworkdata: Step 5 — fetching GP work list {Url}", gpUrl);
            var gpHtml = await NicGetWithRetryAsync(crawlClient, gpUrl);
            if (string.IsNullOrWhiteSpace(gpHtml))
            {
                _log.LogWarning("getworkdata: Empty GP page response");
                return Ok(new Dictionary<string, string[]>());
            }
            doc = new HtmlDocument();
            doc.LoadHtml(gpHtml);

            var workRows = doc.DocumentNode.SelectNodes("/html[1]/body[1]/form[1]/div[3]/center[1]/table[1]/tr");
            var workJson = new Dictionary<string, string[]>();

            if (workRows != null)
            {
                foreach (var row in workRows)
                {
                    var workDoc = new HtmlDocument();
                    workDoc.LoadHtml(row.InnerHtml);
                    var tds = workDoc.DocumentNode.SelectNodes("//td");
                    var workAnchors = workDoc.DocumentNode.SelectNodes("//a");

                    if (workAnchors != null && workAnchors.Count > 0)
                    {
                        var rawHref = workAnchors[0].Attributes["href"]?.Value ?? "";
                        var workLink = ResolveUrl(gpUrl, rawHref);
                        if (string.IsNullOrEmpty(workLink)) continue;
                        try
                        {
                            var parms = HttpUtility.ParseQueryString(new Uri(workLink).Query);
                            var wkCode = parms.Get("work_code");
                            if (!string.IsNullOrEmpty(wkCode))
                            {
                                var wkName = parms.Get("work_name") ?? "";
                                var wkStatus = tds != null && tds.Count > 2
                                    ? tds[2].InnerText.Trim() : "";
                                workJson[wkCode.ToUpper()] = new[]
                                {
                                    wkName,
                                    wkStatus,
                                    workLink,
                                    wkCode.ToUpper()
                                };
                            }
                        }
                        catch { }
                    }
                }
            }

            _log.LogInformation("getworkdata: Found {Count} works for GP {GP}", workJson.Count, panchayat_code);
            return Ok(workJson);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "getworkdata failed for gp={GP}", panchayat_code);
            return StatusCode(502, new { error = "Failed to fetch data from NIC server. Please try again." });
        }
    }

    // ── GET /api/proxy/getnmrdata ──────────────────────────────────
    // Replaces: /api/getWorkAsset.aspx + /api/getNmrData.aspx
    //
    // Mirrors getWorkAsset.aspx.cs exactly:
    //   1. GET  nregasearch1.aspx               → initial ViewState
    //   2. POST ddl_search=Work                 → select work search type
    //   3. POST ddl_state=15                    → select Karnataka
    //   4. POST ddl_district=district_code      → select district
    //   5. POST txt_keyword2=work_code&btn_go=GO → submit work code search
    //   6. Extract &Digest= from response
    //   7. GET  master_search1.aspx?wsrch=wrk&srch=work_code&Digest=...
    //   8. Follow 3rd link → work asset page with NMR msrno links
    //   9. Parse and return NMR list as JSON
    //
    // Falls back to nmrindex.aspx if district_code is absent (deltacheck reuse).
    [HttpGet("getnmrdata")]
    public async Task<IActionResult> GetNmrData(
        [FromQuery] string work_code,
        [FromQuery] string district_code  = "",
        [FromQuery] string district_name  = "",
        [FromQuery] string fin_year       = "2025-2026")
    {
        try
        {
            // ── Fallback: no district code → use nmrindex directly (deltacheck path) ──
            if (string.IsNullOrWhiteSpace(district_code))
            {
                using var fallbackClient = CreateNicClient();
                var fbUrl = $"https://nregastrep.nic.in/netnrega/nmrindex.aspx?state_code=15" +
                            $"&state_name=KARNATAKA&work_code={Uri.EscapeDataString(work_code)}" +
                            $"&fin_year={Uri.EscapeDataString(fin_year)}&lflag=eng";
                var fbHtml = await NicGetWithRetryAsync(fallbackClient, fbUrl);
                var fbList = string.IsNullOrWhiteSpace(fbHtml)
                    ? new List<NmrEntry>()
                    : ParseNmrLinks(fbHtml, fbUrl);
                return Ok(new { nmrList = fbList });
            }

            // ── 4-step ViewState crawl (mirrors getWorkAsset.aspx.cs) ─────────────
            const string searchUrl = "https://mnregaweb4.nic.in/netnrega/nregasearch1.aspx";
            using var c = CreateNicClient();

            // Step 0: GET initial page for ViewState + as_fid/as_sfid
            var s0html = await NicGetWithRetryAsync(c, searchUrl);
            var s0doc  = new HtmlDocument(); s0doc.LoadHtml(s0html);

            string Vs(HtmlDocument d, string id) =>
                d.GetElementbyId(id)?.GetAttributeValue("value", "") ?? "";
            string Inp(HtmlDocument d, string name) =>
                d.DocumentNode.SelectSingleNode($"//input[@name='{name}']")
                    ?.GetAttributeValue("value", "") ?? "";
            // as_fid / as_sfid are read once from the initial page and reused as-is
            // (matches original: commented-out re-reads in steps 2–4)
            var asFid  = Inp(s0doc, "as_fid");
            var asSfid = Inp(s0doc, "as_sfid");

            string TokRaw(string[] parts, string key)
            {
                var idx = Array.IndexOf(parts, key);
                return idx >= 0 && idx + 1 < parts.Length ? parts[idx + 1] : "";
            }

            // Step 1: POST ddl_search=Work
            var p1 = new Dictionary<string, string>
            {
                ["ScriptManager1"]       = "UpdatePanel1|ddl_search",
                ["__ASYNCPOST"]          = "true",
                ["__EVENTTARGET"]        = "ddl_search",
                ["__EVENTARGUMENT"]      = "",
                ["__LASTFOCUS"]          = "",
                ["__VIEWSTATE"]          = Vs(s0doc, "__VIEWSTATE"),
                ["__VIEWSTATEGENERATOR"] = Vs(s0doc, "__VIEWSTATEGENERATOR"),
                ["__VIEWSTATEENCRYPTED"] = "",
                ["__EVENTVALIDATION"]    = Vs(s0doc, "__EVENTVALIDATION"),
                ["as_fid"]               = asFid,
                ["as_sfid"]              = asSfid,
                ["ddl_search"]           = "Work",
                ["ddl_state"]            = "Select",
                ["districtname"]         = "",
                ["hhshortname"]          = "",
                ["searchname"]           = "",
                ["statename"]            = "",
                ["txt_keyword2"]         = ""
            };
            var r1str = await (await c.PostAsync(searchUrl, new FormUrlEncodedContent(p1)))
                            .Content.ReadAsStringAsync();
            var r1 = r1str.Split('|');

            // Step 2: POST ddl_state=15
            var p2 = new Dictionary<string, string>(p1)
            {
                ["ScriptManager1"]       = "UpdatePanel1|ddl_state",
                ["__EVENTTARGET"]        = "ddl_state",
                ["__VIEWSTATE"]          = TokRaw(r1, "__VIEWSTATE"),
                ["__VIEWSTATEGENERATOR"] = TokRaw(r1, "__VIEWSTATEGENERATOR"),
                ["__VIEWSTATEENCRYPTED"] = "",
                ["__EVENTVALIDATION"]    = TokRaw(r1, "__EVENTVALIDATION"),
                ["ddl_state"]            = "15"
                // as_fid / as_sfid intentionally NOT updated (mirrors .aspx.cs commented-out lines)
            };
            var r2str = await (await c.PostAsync(searchUrl, new FormUrlEncodedContent(p2)))
                            .Content.ReadAsStringAsync();
            var r2 = r2str.Split('|');

            // Step 3: POST ddl_district=district_code
            var p3 = new Dictionary<string, string>(p2)
            {
                ["ScriptManager1"]       = "UpdatePanel1|ddl_district",
                ["__EVENTTARGET"]        = "ddl_district",
                ["__VIEWSTATE"]          = TokRaw(r2, "__VIEWSTATE"),
                ["__VIEWSTATEGENERATOR"] = TokRaw(r2, "__VIEWSTATEGENERATOR"),
                ["__EVENTVALIDATION"]    = TokRaw(r2, "__EVENTVALIDATION"),
                ["ddl_district"]         = district_code
            };
            var r3str = await (await c.PostAsync(searchUrl, new FormUrlEncodedContent(p3)))
                            .Content.ReadAsStringAsync();
            var r3 = r3str.Split('|');

            // Step 4: POST txt_keyword2=work_code + btn_go=GO (no __ASYNCPOST)
            var p4 = new Dictionary<string, string>(p3)
            {
                ["ScriptManager1"]       = "UpdatePanel1|ddl_district",
                ["__EVENTTARGET"]        = "",
                ["__VIEWSTATE"]          = TokRaw(r3, "__VIEWSTATE"),
                ["__VIEWSTATEGENERATOR"] = TokRaw(r3, "__VIEWSTATEGENERATOR"),
                ["__EVENTVALIDATION"]    = TokRaw(r3, "__EVENTVALIDATION"),
                ["districtname"]         = district_name,
                ["hhshortname"]          = "KN",
                ["searchname"]           = "Work",
                ["statename"]            = "KARNATAKA",
                ["txt_keyword2"]         = work_code,
                ["btn_go"]               = "GO"
            };
            p4.Remove("__ASYNCPOST");   // final submit is a normal POST, not async
            var r4str = await (await c.PostAsync(searchUrl, new FormUrlEncodedContent(p4)))
                            .Content.ReadAsStringAsync();

            // Extract &Digest= token (matches original IndexOf logic)
            int digestIdx = r4str.IndexOf("&Digest=",        StringComparison.Ordinal);
            int endIdx    = r4str.IndexOf("', 'popup_window',", StringComparison.Ordinal);
            if (digestIdx < 0 || endIdx <= digestIdx)
            {
                _log.LogWarning("getnmrdata: no Digest in step-4 response for work={Work}", work_code);
                return Ok(new { nmrList = new List<NmrEntry>() });
            }
            var digest = r4str.Substring(digestIdx, endIdx - digestIdx);

            // Step 5: GET master_search1 results page
            var masterUrl = $"https://mnregaweb4.nic.in/netnrega/master_search1.aspx" +
                            $"?flag=2&wsrch=wrk" +
                            $"&district_code={district_code}" +
                            $"&state_name=KARNATAKA" +
                            $"&district_name={Uri.EscapeDataString(district_name)}" +
                            $"&short_name=KN" +
                            $"&srch={Uri.EscapeDataString(work_code)}" +
                            digest;

            var masterHtml = await NicGetWithRetryAsync(c, masterUrl);
            var masterDoc  = new HtmlDocument(); masterDoc.LoadHtml(masterHtml);
            var links      = masterDoc.DocumentNode.SelectNodes("//a");

            // 3rd link (index 2) is the work detail/asset page — mirrors .aspx.cs [2]
            if (links == null || links.Count < 3)
            {
                _log.LogWarning("getnmrdata: master_search1 returned <3 links for work={Work}", work_code);
                return Ok(new { nmrList = new List<NmrEntry>() });
            }

            var assetPath = links[2].GetAttributeValue("href", "");
            var assetUrl  = "https://mnregaweb4.nic.in/netnrega/" + assetPath;

            // Step 6: GET the work asset page — this lists all NMRs with msrno= links
            var assetHtml = await NicGetWithRetryAsync(c, assetUrl);
            if (string.IsNullOrWhiteSpace(assetHtml))
                return Ok(new { nmrList = new List<NmrEntry>(), assetUrl });

            var nmrList = ParseNmrLinks(assetHtml, assetUrl);
            return Ok(new { nmrList, assetUrl });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "getnmrdata failed for work={Work}", work_code);
            return StatusCode(502, new { error = "Failed to fetch NMR data." });
        }
    }

    // ── POST /api/proxy/deltacheck ─────────────────────────────────
    // Replaces: /templates/deltacheck (original ASPX UpdatePanel response)
    // Body: comma-separated NMR numbers already loaded client-side
    // Returns: { newNmrs: [...] } — only NMRs not in the supplied list
    [HttpPost("deltacheck")]
    public async Task<IActionResult> DeltaCheck(
        [FromQuery] string work_code,
        [FromQuery] string fin_year = "2025-2026")
    {
        try
        {
            // Read loaded NMR numbers from POST body
            string body;
            using (var reader = new StreamReader(Request.Body))
                body = await reader.ReadToEndAsync();

            var loadedSet = (body ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => x.Length > 2)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            using var client = CreateNicClient();
            var nicUrl = $"https://nregastrep.nic.in/netnrega/nmrindex.aspx?state_code=15" +
                         $"&state_name=KARNATAKA&work_code={Uri.EscapeDataString(work_code)}" +
                         $"&fin_year={Uri.EscapeDataString(fin_year)}&lflag=eng";

            var html = await NicGetWithRetryAsync(client, nicUrl);
            if (string.IsNullOrWhiteSpace(html))
                return Ok(new { newNmrs = Array.Empty<object>() });

            var allNmrs = ParseNmrLinks(html, nicUrl);
            var newNmrs = allNmrs.Where(n => !loadedSet.Contains(n.NmrNo)).ToList();

            return Ok(new { newNmrs });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "deltacheck failed for work={Work}", work_code);
            return Ok(new { newNmrs = Array.Empty<object>() }); // graceful — don't break UI
        }
    }

    // ── Shared: parse <a href> links with msrno param from NIC NMR index page ─
    private static List<NmrEntry> ParseNmrLinks(string html, string pageUrl)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<NmrEntry>();

        var anchors = doc.DocumentNode.SelectNodes("//a[@href]");
        if (anchors == null) return result;

        foreach (var a in anchors)
        {
            var href = a.GetAttributeValue("href", "");
            if (!href.Contains("msrno=", StringComparison.OrdinalIgnoreCase)) continue;

            try
            {
                var fullUrl = new Uri(new Uri(pageUrl), href).ToString();
                var qs = HttpUtility.ParseQueryString(new Uri(fullUrl).Query);
                var msrnoRaw = qs["msrno"]?.Trim() ?? "";
                if (string.IsNullOrEmpty(msrnoRaw)) continue;

                // Dates are NOT read from the workasset page table.
                // They are populated by the client hydrating each NMR via Musternew.aspx
                // (getform8data → _hydrateNmrFromHtml), same as the original parseWorkNmrs flow.

                // NIC sometimes encodes multiple NMRs in one msrno param as a comma-separated
                // list with parenthetical job-count suffixes, e.g. "874(3672),906(3672),919(22032)".
                // Split by comma and create one NmrEntry per NMR number, all sharing the same
                // date range (they belong to the same muster roll period on the workasset page).
                var parts = msrnoRaw.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    var raw = part.Trim();
                    // Strip parenthetical suffix — e.g. "874(3672)" → "874"
                    var parenIdx = raw.IndexOf('(');
                    var msrno = (parenIdx > 0 ? raw[..parenIdx] : raw).Trim();
                    if (string.IsNullOrEmpty(msrno) || seen.Contains(msrno)) continue;
                    seen.Add(msrno);

                    // Build a clean individual URL for this NMR.
                    var baseQs = HttpUtility.ParseQueryString(new Uri(fullUrl).Query);
                    baseQs["msrno"] = msrno;
                    var musterUrl = "https://nregastrep.nic.in/netnrega/citizen_html/Musternew.aspx?" + baseQs;

                    result.Add(new NmrEntry
                    {
                        NmrNo    = msrno,
                        NmrLink  = musterUrl,
                        FromDate = "",
                        ToDate   = "",
                        Status   = ""
                    });
                }
            }
            catch { /* skip malformed href */ }
        }

        return result;
    }

    private record NmrEntry
    {
        [System.Text.Json.Serialization.JsonPropertyName("nmrNo")]
        public string NmrNo    { get; init; } = "";
        [System.Text.Json.Serialization.JsonPropertyName("nmrLink")]
        public string NmrLink  { get; init; } = "";
        [System.Text.Json.Serialization.JsonPropertyName("fromDate")]
        public string FromDate { get; init; } = "";
        [System.Text.Json.Serialization.JsonPropertyName("toDate")]
        public string ToDate   { get; init; } = "";
        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string Status   { get; init; } = "";
    }

    // ── GET /api/proxy/getwagelist ─────────────────────────────────
    // Two roles depending on the nmr_link value:
    //   1. nmr_link = NMR muster roll URL (DPR_Work.aspx?...)
    //      → simple GET passthrough to return the NMR page HTML
    //        (which contains ContentPlaceHolder1_grdShowRecords with wage list IDs)
    //   2. nmr_link = wagelistUrl+str (srch_wg_dtl.aspx?...&wageList=...)
    //      → mirror getWageListData.aspx: 4-step ViewState crawl through
    //        nregasearch1.aspx to find and return the wage list detail HTML
    //   Detection: if nmr_link contains "srch_wg_dtl" or "wageList=" use multi-step,
    //   otherwise simple passthrough.
    [HttpGet("getwagelist")]
    public async Task<IActionResult> GetWageList([FromQuery] string nmr_link)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(nmr_link))
                return BadRequest(new { error = "nmr_link is required" });

            // Simple passthrough for NMR page URLs (step 1 of wage list flow)
            if (!nmr_link.Contains("srch_wg_dtl") && !nmr_link.Contains("wageList="))
            {
                var html = await FetchNicUrl(nmr_link);
                return Ok(new { html });
            }

            // ── Multi-step wage list detail fetch ──────────────────────────────
            // Mirrors getWageListData.aspx.cs exactly.
            // Parses query string from the srch_wg_dtl URL to extract params.
            var qs        = System.Web.HttpUtility.ParseQueryString(new Uri(nmr_link).Query);
            var srch      = qs["srch"]          ?? "";       // date in dd/MM/yyyy
            var wagelistno= qs["wageList"]       ?? qs["wg_no"] ?? "";
            var distCode  = qs["district_code"]  ?? "";
            var distName  = qs["district_name"]  ?? "";
            var blockCode = qs["block_code"]     ?? "";

            if (string.IsNullOrWhiteSpace(wagelistno))
            {
                // Fallback: plain GET for any unrecognised URL shape
                var html = await FetchNicUrl(nmr_link);
                return Ok(new { html });
            }

            // Parse financial years from srch date (dd/MM/yyyy)
            string finYear0 = "", finYear1 = "";
            if (DateTime.TryParseExact(srch, "d/M/yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var srchDate))
            {
                finYear0 = $"{srchDate.Year - 1}-{srchDate.Year}";
                finYear1 = $"{srchDate.Year}-{srchDate.Year + 1}";
            }

            string searchUrl = "https://mnregaweb4.nic.in/netnrega/nregasearch1.aspx";
            using var c = CreateNicClient();

            // ── Step 0: GET nregasearch1 to obtain initial ViewState ──────────
            var s0html = await NicGetWithRetryAsync(c, searchUrl);
            var s0doc  = new HtmlDocument(); s0doc.LoadHtml(s0html);
            string Vs(HtmlDocument d, string id) =>
                Uri.EscapeDataString(d.GetElementbyId(id)?.GetAttributeValue("value","") ?? "");
            string Inp(HtmlDocument d, string name) =>
                Uri.EscapeDataString(d.DocumentNode
                    .SelectSingleNode($"//input[@name='{name}']")
                    ?.GetAttributeValue("value","") ?? "");

            // ── Step 1: POST ddl_search=WageList ─────────────────────────────
            var p1 = new Dictionary<string,string>
            {
                ["ScriptManager1"]       = "UpdatePanel1|ddl_search",
                ["__ASYNCPOST"]          = "true",
                ["__EVENTTARGET"]        = "ddl_search",
                ["__EVENTARGUMENT"]      = "",
                ["__LASTFOCUS"]          = "",
                ["__VIEWSTATE"]          = Uri.UnescapeDataString(Vs(s0doc,"__VIEWSTATE")),
                ["__VIEWSTATEGENERATOR"] = Uri.UnescapeDataString(Vs(s0doc,"__VIEWSTATEGENERATOR")),
                ["__VIEWSTATEENCRYPTED"] = "",
                ["__EVENTVALIDATION"]    = Uri.UnescapeDataString(Vs(s0doc,"__EVENTVALIDATION")),
                ["as_fid"]               = Uri.UnescapeDataString(Inp(s0doc,"as_fid")),
                ["as_sfid"]              = Uri.UnescapeDataString(Inp(s0doc,"as_sfid")),
                ["ddl_search"]           = "WageList",
                ["ddl_state"]            = "Select",
                ["districtname"]         = "",
                ["hhshortname"]          = "",
                ["searchname"]           = "",
                ["statename"]            = "",
                ["txt_keyword2"]         = ""
            };
            var r1str = await (await c.PostAsync(searchUrl,
                new FormUrlEncodedContent(p1))).Content.ReadAsStringAsync();
            var r1 = r1str.Split('|');
            string Tok(string[] parts, string key) =>
                Uri.EscapeDataString(parts.Length > Array.IndexOf(parts,key)+1
                    ? parts[Array.IndexOf(parts,key)+1] : "");

            // ── Step 2: POST ddl_state=15 ─────────────────────────────────────
            var p2 = new Dictionary<string,string>(p1);
            p2["ScriptManager1"]       = "UpdatePanel1|ddl_state";
            p2["__EVENTTARGET"]        = "ddl_state";
            p2["__VIEWSTATE"]          = Uri.UnescapeDataString(Tok(r1,"__VIEWSTATE"));
            p2["__VIEWSTATEGENERATOR"] = Uri.UnescapeDataString(Tok(r1,"__VIEWSTATEGENERATOR"));
            p2["__VIEWSTATEENCRYPTED"] = "";
            p2["__EVENTVALIDATION"]    = Uri.UnescapeDataString(Tok(r1,"__EVENTVALIDATION"));
            p2["ddl_state"]            = "15";
            var r2str = await (await c.PostAsync(searchUrl,
                new FormUrlEncodedContent(p2))).Content.ReadAsStringAsync();
            var r2 = r2str.Split('|');

            // ── Step 3: POST ddl_district=distCode ───────────────────────────
            var p3 = new Dictionary<string,string>(p2);
            p3["ScriptManager1"]       = "UpdatePanel1|ddl_district";
            p3["__EVENTTARGET"]        = "ddl_district";
            p3["__VIEWSTATE"]          = Uri.UnescapeDataString(Tok(r2,"__VIEWSTATE"));
            p3["__VIEWSTATEGENERATOR"] = Uri.UnescapeDataString(Tok(r2,"__VIEWSTATEGENERATOR"));
            p3["__EVENTVALIDATION"]    = Uri.UnescapeDataString(Tok(r2,"__EVENTVALIDATION"));
            p3["ddl_district"]         = distCode;
            var r3str = await (await c.PostAsync(searchUrl,
                new FormUrlEncodedContent(p3))).Content.ReadAsStringAsync();
            var r3 = r3str.Split('|');

            // ── Step 4: POST txt_keyword2=wagelistno + btn_go=GO ─────────────
            var p4 = new Dictionary<string,string>(p3);
            p4["ScriptManager1"]       = "UpdatePanel1|ddl_district";
            p4["__EVENTTARGET"]        = "";
            p4["__VIEWSTATE"]          = Uri.UnescapeDataString(Tok(r3,"__VIEWSTATE"));
            p4["__VIEWSTATEGENERATOR"] = Uri.UnescapeDataString(Tok(r3,"__VIEWSTATEGENERATOR"));
            p4["__EVENTVALIDATION"]    = Uri.UnescapeDataString(Tok(r3,"__EVENTVALIDATION"));
            p4["districtname"]         = distName;
            p4["hhshortname"]          = "KN";
            p4["searchname"]           = "WageList";
            p4["statename"]            = "KARNATAKA";
            p4["txt_keyword2"]         = wagelistno;
            p4["btn_go"]               = "GO";
            var r4str = await (await c.PostAsync(searchUrl,
                new FormUrlEncodedContent(p4))).Content.ReadAsStringAsync();

            // ── Extract Digest and fetch master_search1.aspx ──────────────────
            int digestIdx = r4str.IndexOf("&Digest=", StringComparison.Ordinal);
            int endIdx    = r4str.IndexOf("', 'popup_window',", StringComparison.Ordinal);
            if (digestIdx < 0 || endIdx < digestIdx)
            {
                _log.LogWarning("getwagelist: no Digest found in step-4 response for wageList={WL}", wagelistno);
                return Ok(new { html = r4str });
            }
            var digest = r4str.Substring(digestIdx, endIdx - digestIdx);
            var wgUrl  = $"https://mnregaweb4.nic.in/netnrega/master_search1.aspx?flag=2&wsrch=wg" +
                         $"&district_code={distCode}&state_name=KARNATAKA&district_name={Uri.EscapeDataString(distName)}" +
                         $"&short_name=KN&srch={Uri.EscapeDataString(wagelistno)}{digest}";

            var wg1html = await NicGetWithRetryAsync(c, wgUrl);
            var wgDoc   = new HtmlDocument(); wgDoc.LoadHtml(wg1html);
            var wgLinks = wgDoc.DocumentNode.SelectNodes("//a");

            if (wgLinks != null && wgLinks.Count >= 4)
            {
                var wgPage = "https://mnregaweb4.nic.in/netnrega/" + wgLinks[2].GetAttributeValue("href","");
                var finalHtml = await NicGetWithRetryAsync(c, wgPage);
                return Ok(new { html = finalHtml });
            }

            // ── Fallback: try each financial year via ddl_yr POST ─────────────
            foreach (var fy in new[] { finYear0, finYear1 }.Where(f => !string.IsNullOrWhiteSpace(f)))
            {
                var vs  = Uri.UnescapeDataString(Vs(wgDoc,"__VIEWSTATE"));
                var vsg = Uri.UnescapeDataString(Vs(wgDoc,"__VIEWSTATEGENERATOR"));
                var ev  = Uri.UnescapeDataString(Vs(wgDoc,"__EVENTVALIDATION"));
                if (string.IsNullOrWhiteSpace(vs)) break;

                var py = new Dictionary<string,string>
                {
                    ["__EVENTTARGET"]        = "ddl_yr",
                    ["__EVENTARGUMENT"]      = "",
                    ["__LASTFOCUS"]          = "",
                    ["__VIEWSTATE"]          = vs,
                    ["__VIEWSTATEGENERATOR"] = vsg,
                    ["__VIEWSTATEENCRYPTED"] = "",
                    ["__EVENTVALIDATION"]    = ev,
                    ["ddl_yr"]               = fy
                };
                var fyHtml = await (await c.PostAsync(wgUrl,
                    new FormUrlEncodedContent(py))).Content.ReadAsStringAsync();
                wgDoc.LoadHtml(fyHtml);
                var fyLinks = wgDoc.DocumentNode.SelectNodes("//a");
                if (fyLinks != null && fyLinks.Count >= 4)
                {
                    var wgPage2 = "https://mnregaweb4.nic.in/netnrega/" + fyLinks[2].GetAttributeValue("href","");
                    var finalHtml2 = await NicGetWithRetryAsync(c, wgPage2);
                    return Ok(new { html = finalHtml2 });
                }
            }

            _log.LogWarning("getwagelist: no wage list link found for {WL}", wagelistno);
            return Ok(new { html = wg1html });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "getwagelist failed for nmr_link={Link}", nmr_link);
            return StatusCode(502, new { error = "Failed to fetch wage list." });
        }
    }

    // ── GET /api/proxy/workdetail ──────────────────────────────────
    // Replaces: /api/wagelistdata?dpr=true (original POST with NIC URL body)
    // Fetches the NIC DPCApprove_Workdetail.aspx page for a specific work
    // and returns parsed JSON fields (startdate, finSanctionNo, etc.)
    // nic_url comes from DPRresponse[workCode][2] on the client
    [HttpGet("workdetail")]
    public async Task<IActionResult> GetWorkDetail([FromQuery] string nic_url)
    {
        if (string.IsNullOrWhiteSpace(nic_url))
            return BadRequest(new { error = "nic_url is required" });
        try
        {
            using var client = CreateNicClient();
            var html = await NicGetWithRetryAsync(client, nic_url);
            if (string.IsNullOrWhiteSpace(html))
                return Ok(new { error = "Empty response from NIC" });

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            string GetText(string id)
            {
                var node = doc.GetElementbyId(id);
                return (node?.InnerText ?? "").Trim();
            }

            // Material table rows — mirrors Bind_UpdateWorkDetails GridView1 parsing
            var materials = new List<object>();
            var grid = doc.GetElementbyId("ctl00_ContentPlaceHolder1_GridView1");
            if (grid != null)
            {
                var rows = grid.SelectNodes(".//tr");
                if (rows != null)
                {
                    foreach (var row in rows.Skip(1)) // skip header row
                    {
                        var cells = row.SelectNodes(".//td");
                        if (cells != null && cells.Count >= 5)
                            materials.Add(new {
                                Material  = cells[1].InnerText.Trim(),
                                Quantity  = cells[2].InnerText.Trim(),
                                UnitPrice = cells[3].InnerText.Trim(),
                                Total     = cells[4].InnerText.Trim()
                            });
                    }
                }
            }

            return Ok(new {
                startdate        = GetText("ctl00_ContentPlaceHolder1_LblWrksdate"),
                workCategory     = GetText("ctl00_ContentPlaceHolder1_catlbl"),
                workYear         = GetText("ctl00_ContentPlaceHolder1_lblfin_text"),
                workCostTotal    = GetText("ctl00_ContentPlaceHolder1_Lblfin_total"),
                executingAgency  = GetText("ctl00_ContentPlaceHolder1_lbl_agency_text"),
                executionLevel   = GetText("ctl00_ContentPlaceHolder1_ExeLevel_text"),
                workStatus       = GetText("ctl00_ContentPlaceHolder1_lblworkstatus_text"),
                finSanctionNo    = GetText("ctl00_ContentPlaceHolder1_Lblsanc_fin_no"),
                finSanctionDate  = GetText("ctl00_ContentPlaceHolder1_Lblsanc_fin_dt"),
                techSanctionNo   = GetText("ctl00_ContentPlaceHolder1_lblsanctionno_text"),
                techSanctionDate = GetText("ctl00_ContentPlaceHolder1_lblsandate_text"),
                UskilledExp      = GetText("ctl00_ContentPlaceHolder1_lblSanc_Tech_Labr_Unskilled"),
                MaterialCost     = GetText("ctl00_ContentPlaceHolder1_lblEst_Cost_Material"),
                SkilledCost      = GetText("ctl00_ContentPlaceHolder1_Lblskill"),
                Material         = materials
            });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "workdetail failed for url={Url}", nic_url);
            return StatusCode(502, new { error = "Failed to fetch work detail from NIC." });
        }
    }

    // ── GET /api/proxy/getftodetails ───────────────────────────────
    // Replaces: /api/getftoDetails.aspx
    // Mirrors getftoDetails.aspx.cs exactly:
    //   1. Parse financial year from FTO number (month 1-3 → previous year)
    //   2. Navigate state→district→block→panchayat→FTO link
    //   3. Return raw FTO detail HTML
    [HttpGet("getftodetails")]
    public async Task<IActionResult> GetFtoDetails(
        [FromQuery] string fto_no,
        [FromQuery] string district_code,
        [FromQuery] string block_code,
        [FromQuery] string panchayat_code)
    {
        try
        {
            // Digest map keyed by financial year (mirrors original dictionary)
            var digestMap = new Dictionary<string,string>
            {
                ["2025-2026"] = "VIYEYkV6KCjmigpCauTElQ",
                ["2024-2025"] = "G5nkV/MnRcIFaFkhI3Hsyw",
                ["2023-2024"] = "0Q3J/VJe0jM6Dsi8JF5ueA",
                ["2022-2023"] = "QCziIGEXM4BBB2VukVqkOQ",
                ["2021-2022"] = "3eCaVeN5tPmW91mkdhTBjg",
                ["2020-2021"] = "0bInl8ptge+QQGaJcC+Wow",
                ["2019-2020"] = "6yAWOrQeWWvv5uerhRvImA"
            };

            // Parse financial year from FTO number format: prefix_DDMMYYsuffix
            // Part[1] positions 2-3 = month, 4-5 = 2-digit year
            var parts   = (fto_no ?? "").Split('_');
            string finYear;
            if (parts.Length >= 2 && parts[1].Length >= 6 &&
                int.TryParse(parts[1].Substring(2, 2), out int month) &&
                int.TryParse(parts[1].Substring(4, 2), out int yr2))
            {
                finYear = month >= 1 && month <= 3
                    ? $"20{yr2 - 1:D2}-20{yr2:D2}"
                    : $"20{yr2:D2}-20{yr2 + 1:D2}";
            }
            else
            {
                finYear = "2024-2025";
            }

            if (!digestMap.TryGetValue(finYear, out var digest))
                digest = digestMap["2024-2025"];

            string baseUrl  = "https://mnregaweb4.nic.in/netnrega/FTO/";
            string stateUrl = baseUrl +
                $"FTOReport.aspx?page=s&mode=B&flg=W&state_name=KARNATAKA&state_code=15" +
                $"&fin_year={finYear}&dstyp=B&source=national&Digest={Uri.EscapeDataString(digest)}";

            using var c  = CreateNicClient();
            var doc      = new HtmlDocument();

            // Helper: get param value from absolute-ified href
            string? GetParam(string href, string key)
            {
                try
                {
                    var abs = href.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                              ? href : baseUrl + href;
                    return System.Web.HttpUtility.ParseQueryString(new Uri(abs).Query)[key];
                }
                catch { return null; }
            }

            // ── Level 1: State → find district link ───────────────────────────
            doc.LoadHtml(await NicGetWithRetryAsync(c, stateUrl));
            var distLinks = doc.DocumentNode.SelectNodes("//table[2]//td[2]//a");
            var distLink  = distLinks?.FirstOrDefault(a =>
                GetParam(a.GetAttributeValue("href",""), "district_code") == district_code);
            if (distLink == null)
                return StatusCode(502, new { error = $"District {district_code} not found in FTO report" });

            // ── Level 2: District → find block link ───────────────────────────
            doc.LoadHtml(await NicGetWithRetryAsync(c, baseUrl + distLink.GetAttributeValue("href","")));
            var blkLinks = doc.DocumentNode.SelectNodes("//table[2]//td[2]//a");
            var blkLink  = blkLinks?.FirstOrDefault(a =>
                GetParam(a.GetAttributeValue("href",""), "block_code") == block_code);
            if (blkLink == null)
                return StatusCode(502, new { error = $"Block {block_code} not found in FTO report" });

            // ── Level 3: Block → find panchayat link ──────────────────────────
            doc.LoadHtml(await NicGetWithRetryAsync(c, baseUrl + blkLink.GetAttributeValue("href","")));
            var gpLinks = doc.DocumentNode.SelectNodes("//table[2]//td[4]//a");
            var gpLink  = gpLinks?.FirstOrDefault(a =>
                GetParam(a.GetAttributeValue("href",""), "panchayat_code") == panchayat_code);
            if (gpLink == null)
                return StatusCode(502, new { error = $"Panchayat {panchayat_code} not found in FTO report" });

            // ── Level 4: Panchayat → find specific FTO link ───────────────────
            doc.LoadHtml(await NicGetWithRetryAsync(c, baseUrl + gpLink.GetAttributeValue("href","")));
            var ftoLinks = doc.DocumentNode.SelectNodes("//a");
            var ftoLink  = ftoLinks?.FirstOrDefault(a =>
                GetParam(a.GetAttributeValue("href",""), "fto_no") == fto_no);
            if (ftoLink == null)
                return StatusCode(502, new { error = $"FTO {fto_no} not found" });

            var finalHtml = await NicGetWithRetryAsync(c, baseUrl + ftoLink.GetAttributeValue("href",""));
            return Ok(new { html = finalHtml });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "getftodetails failed for fto_no={FTO}", fto_no);
            return StatusCode(502, new { error = "Failed to fetch FTO details." });
        }
    }

    // ── POST /api/proxy/converter ──────────────────────────────────
    // Replaces: /templates/converter?page=...
    // Receives raw HTML in request body, converts to PDF via EvoPDF.
    // Uses the same license key and API as the original converter.aspx.cs.
    // Query params: page (WageList|FTO|filledNmr|blknmr|GeoTag|...), workcode, WageList, FTO, stateName
    [HttpPost("converter")]
    public async Task<IActionResult> Converter(
        [FromQuery] string  page,
        [FromQuery] string? workcode  = null,
        [FromQuery] string? WageList  = null,
        [FromQuery] string? FTO       = null,
        [FromQuery] string? stateName = null)
    {
        try
        {
            string html;
            using (var reader = new StreamReader(Request.Body, System.Text.Encoding.UTF8))
                html = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(html))
                return BadRequest(new { error = "Empty HTML body" });

            // ── EvoPDF setup — mirrors converter.aspx.cs exactly ──────────────
            HtmlToPdfConverter htmlToPdfConverter = new HtmlToPdfConverter();
            htmlToPdfConverter.LicenseKey = "aOb25/T05/Pz8PLn/+n35/T26fb16f7+/v7n9w==";
           
            htmlToPdfConverter.ConversionDelay = 0;
            htmlToPdfConverter.PdfDocumentOptions.LeftMargin  = 5;
            htmlToPdfConverter.PdfDocumentOptions.RightMargin = 5;
            htmlToPdfConverter.PdfDocumentOptions.TopMargin   = 5;

            // Landscape for FTO, WageList, blknmr, filledNmr — mirrors original switch
            bool landscape = page is "FTO" or "WageList" or "blknmr" or "filledNmr";
            if (landscape)
                htmlToPdfConverter.PdfDocumentOptions.PdfPageOrientation = EvoPdf.PdfPageOrientation.Portrait;

            // Base URL so relative image/CSS paths in NIC HTML resolve correctly
            string baseUrl = page is "WageList" or "FTO"
                ? "https://mnregaweb4.nic.in/netnrega/"
                : $"{Request.Scheme}://{Request.Host}/";

            byte[] pdfBytes = htmlToPdfConverter.ConvertHtml(html, baseUrl);

            var ts = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            var wc = workcode ?? "doc";
            string fileName = page switch
            {
                "WageList"  => $"{WageList}_{ts}_WageList.pdf",
                "FTO"       => $"{FTO}_{ts}_FTO.pdf",
                "filledNmr" => $"{wc}_{ts}_filledNMR.pdf",
                "blknmr"    => $"{wc}_{ts}_blanknmr.pdf",
                "GeoTag"    => $"{wc}_{ts}_geotag.pdf",
                _           => $"{wc}_{ts}_{page}.pdf"
            };

            return File(pdfBytes, "application/pdf",
                System.Net.WebUtility.UrlEncode(fileName));
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "converter failed for page={Page}", page);
            return StatusCode(500, new { error = "PDF generation failed: " + ex.Message });
        }
    }

    // NOTE: GET /api/proxy/geotag is handled by GeotagController.cs
    //       (moved to a separate file because ProxyController.cs is already large).

    // ── GET /api/proxy/getform8data ────────────────────────────────
    // Replaces: /api/getForm8Data.aspx
    [HttpGet("getform8data")]
    public async Task<IActionResult> GetForm8Data([FromQuery] string nmr_link)
    {
        try
        {
            var html = await FetchNicUrl(nmr_link);
            return Ok(new { html });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "getform8data failed");
            return StatusCode(502, new { error = "Failed to fetch Form 8 data." });
        }
    }

    // ── Shared helper: fetch a NIC URL that may be absolute or relative ──
    // ParseNmrLinks returns full absolute URLs; some datasync entries store
    // only relative paths. _nic.GetAsync always prepends the NIC base URL,
    // so we must detect absolute URLs and fetch them directly.
    private async Task<string> FetchNicUrl(string nmr_link)
    {
        var decoded = Uri.UnescapeDataString(nmr_link ?? "");
        if (decoded.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            decoded.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            // Already absolute — use a direct HttpClient call, not _nic.GetAsync
            // (which would double-prepend the base URL)
            using var client = CreateNicClient();
            _log.LogInformation("FetchNicUrl (absolute): {Url}", decoded);
            var resp = await client.GetAsync(decoded);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadAsStringAsync();
        }
        // Relative path — let _nic.GetAsync prepend the base URL as normal
        return await _nic.GetAsync(decoded);
    }

    // ── GET /api/proxy/getagencyworkdata ───────────────────────────
    // Replaces: /api/getAgencyworkdata.aspx
    [HttpGet("getagencyworkdata")]
    public async Task<IActionResult> GetAgencyWorkData(
        [FromQuery] string block_code,
        [FromQuery] string fin_year = "2025-2026",
        [FromQuery] string agency = "")
    {
        try
        {
            var html = await _nic.GetAsync(
                $"homestciti.aspx?state_code=15&state_name=KARNATAKA" +
                $"&block_code={block_code}&fin_year={fin_year}" +
                $"&agency={Uri.EscapeDataString(agency)}&lflag=eng");
            return Ok(new { html });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "getagencyworkdata failed");
            return StatusCode(502, new { error = "Failed to fetch agency work data." });
        }
    }

    // ── GET /api/proxy/getagencyasset ──────────────────────────────
    [HttpGet("getagencyasset")]
    public async Task<IActionResult> GetAgencyAsset(
        [FromQuery] string work_code,
        [FromQuery] string fin_year = "2025-2026")
    {
        try
        {
            var html = await _nic.GetAsync(
                $"asset.aspx?state_code=15&work_code={Uri.EscapeDataString(work_code)}" +
                $"&fin_year={fin_year}&lflag=eng");
            return Ok(new { html });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "getagencyasset failed");
            return StatusCode(502, new { error = "Failed to fetch asset data." });
        }
    }

    // ── GET /api/proxy/getworkasset ────────────────────────────────
    [HttpGet("getworkasset")]
    public async Task<IActionResult> GetWorkAsset([FromQuery] string work_code,
        [FromQuery] string fin_year = "2025-2026")
    {
        try
        {
            var html = await _nic.GetAsync(
                $"asset.aspx?state_code=15&work_code={Uri.EscapeDataString(work_code)}" +
                $"&fin_year={fin_year}&lflag=eng");
            return Ok(new { html });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "getworkasset failed");
            return StatusCode(502, new { error = "Failed to fetch work asset data." });
        }
    }

    // ── GET /api/proxy/citizenworkasset ───────────────────────────────
    // Used for IAY/PMIAY works: fetches citizen_html/workasset.aspx with full GP params.
    // Returns { html } — JS parses work details and NMR links from the HTML.
    [HttpGet("citizenworkasset")]
    public async Task<IActionResult> GetCitizenWorkAsset(
        [FromQuery] string wkcode,
        [FromQuery] string panchayat_code,
        [FromQuery] string block_code,
        [FromQuery] string district_code,
        [FromQuery] string panchayat_name = "",
        [FromQuery] string block_name = "",
        [FromQuery] string district_name = "")
    {
        try
        {
            var url = "citizen_html/workasset.aspx" +
                $"?block_code={Uri.EscapeDataString(block_code)}" +
                $"&Panchayat_Code={Uri.EscapeDataString(panchayat_code)}" +
                $"&wkcode={Uri.EscapeDataString(wkcode)}" +
                $"&state_name=KARNATAKA" +
                $"&district_name={Uri.EscapeDataString(district_name)}" +
                $"&block_name={Uri.EscapeDataString(block_name)}" +
                $"&panchayat_name={Uri.EscapeDataString(panchayat_name)}" +
                $"&Digest=s3kn";
            var html = await _nic.GetAsync(url);
            return Ok(new { html });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "citizenworkasset failed for {wkcode}", wkcode);
            return StatusCode(502, new { error = "Failed to fetch work asset page." });
        }
    }

    // ── GET /api/proxy/loadvendordetails ──────────────────────────────
    // Mirrors original LoadVendorDetails.aspx.cs:
    //   1. Fetch district vendor freeze report index
    //   2. Find the link for the requested district_code
    //   3. Fetch that page and parse vendor GST + name into JSON array
    // Returns JSON array: [{ value: "GST", label: "VendorName" }, ...]
    [HttpGet("loadvendordetails")]
    public async Task<IActionResult> LoadVendorDetails([FromQuery] string dist_code)
    {
        try
        {
            // Step 1: fetch state-level vendor freeze index to find the district link
            var indexHtml = await _nic.GetAsync(
                "state_html/freez_vendor_rpt.aspx?lflag=eng&state_code=15&state_name=KARNATAKAeng" +
                "&fin_year=2025-2026&page=s&typ=R&Digest=s3kn");

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(indexHtml);
            var distLink = "";
            foreach (var a in doc.DocumentNode.SelectNodes("//a") ?? Enumerable.Empty<HtmlAgilityPack.HtmlNode>())
            {
                var href = a.GetAttributeValue("href", "");
                var qs = System.Web.HttpUtility.ParseQueryString(
                    href.Contains('?') ? href.Substring(href.IndexOf('?')) : "");
                if (qs["district_code"] == dist_code)
                {
                    distLink = "https://nregastrep.nic.in/netnrega/state_html/" + href;
                    break;
                }
            }
            if (string.IsNullOrEmpty(distLink))
                return Ok(new object[0]);

            // Step 2: fetch district vendor page and parse table rows
            var distHtml = await _nic.GetAsync(distLink.Replace("https://nregastrep.nic.in/netnrega/", ""));
            doc.LoadHtml(distHtml);
            var rows = doc.DocumentNode.SelectNodes("//table[1]//tr");
            if (rows == null) return Ok(new object[0]);

            var vendors = new List<object>();
            for (int i = 2; i < rows.Count; i++)
            {
                var cells = rows[i].SelectNodes("td");
                if (cells != null && cells.Count >= 6)
                    vendors.Add(new { value = cells[3].InnerText.Trim(), label = cells[5].InnerText.Trim() });
            }
            return Ok(vendors);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "loadvendordetails failed for dist_code={dist_code}", dist_code);
            return StatusCode(502, new { error = "Failed to load vendor details." });
        }
    }

    // ── GET /api/proxy/getagencyfto ────────────────────────────────
    [HttpGet("getagencyfto")]
    public async Task<IActionResult> GetAgencyFto([FromQuery] string work_code,
        [FromQuery] string fin_year = "2025-2026")
    {
        try
        {
            var html = await _nic.GetAsync(
                $"fto_check.aspx?state_code=15&work_code={Uri.EscapeDataString(work_code)}" +
                $"&fin_year={fin_year}&lflag=eng");
            return Ok(new { html });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "getagencyfto failed");
            return StatusCode(502, new { error = "Failed to fetch agency FTO." });
        }
    }

    // ── GET /api/proxy/jobcardhh ───────────────────────────────────
    [HttpGet("jobcardhh")]
    public async Task<IActionResult> GetJobCardHH([FromQuery] string panchayat_code,
        [FromQuery] string fin_year = "2025-2026")
    {
        try
        {
            var html = await _nic.GetAsync(
                $"jobcard_register.aspx?state_code=15&panchayat_code={panchayat_code}" +
                $"&fin_year={fin_year}&lflag=eng");
            return Ok(new { html });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "jobcardhh failed");
            return StatusCode(502, new { error = "Failed to fetch job card data." });
        }
    }

    // ── GET /api/proxy/getcatwisejobcards ─────────────────────────
    // Replaces: /api/getcatwisejobcards.aspx
    // Original flow:
    //   1. GET IndexFrame.aspx → session cookie + find "Registration Caste Wise" link
    //   2. GET PoIndexFrame.aspx → PO session + find emuster/disabled/scst links
    //   3. Fetch all 4 data pages with session cookies
    //   Returns: regcat_node $$ muster_html $$ disabled_html $$ scst_html
    [HttpGet("getcatwisejobcards")]
    public async Task<IActionResult> GetCatwiseJobCards(
        [FromQuery] string? Panchayat_Code,
        [FromQuery] string? panchayat_code,
        [FromQuery] string? block_code = "",
        [FromQuery] string? dist_code = "",
        [FromQuery] string? block_name = "",
        [FromQuery] string? dist_name = "",
        [FromQuery] string? panchayatname = "",
        [FromQuery] string fin_year = "2025-2026")
    {
        var pc = Panchayat_Code ?? panchayat_code ?? "";
        try
        {
            var nicBase = "https://nregastrep.nic.in/netnrega/";

            // Step 1: GET IndexFrame.aspx → get session cookie + find "Registration Caste Wise" link
            _log.LogInformation("getcatwisejobcards: Step 1 — IndexFrame for GP {GP}", pc);
            var indexUrl = $"{nicBase}IndexFrame.aspx?lflag=eng" +
                $"&District_Code={dist_code}&district_name={Uri.EscapeDataString(dist_name ?? "")}" +
                $"&state_name=KARNATAKA&state_Code=15" +
                $"&block_name={Uri.EscapeDataString(block_name ?? "")}&block_code={block_code}" +
                $"&fin_year={fin_year}&check=1" +
                $"&Panchayat_name={Uri.EscapeDataString(panchayatname ?? "")}&Panchayat_Code={pc}";

            var indexCookies = new CookieContainer();
            using var indexClient = CreateNicClient(indexCookies);  // 60s, with cookies

            var indexResp = await indexClient.GetAsync(indexUrl);
            var indexHtml = await indexResp.Content.ReadAsStringAsync();

            // Extract session cookie — HttpClientHandler stores cookies in the container automatically.
            // Set-Cookie header is consumed by the handler, so read from CookieContainer directly.
            // Matches original: Indexindex.Headers.Get("Set-Cookie").Split('=')[1].Split(';')[0]
            string indexSession = "";
            try
            {
                var nicUri = new Uri("https://nregastrep.nic.in/");
                var sc = indexCookies.GetCookies(nicUri)["ASP.NET_SessionId"];
                indexSession = sc?.Value ?? "";
            }
            catch { }

            // Find "Registration Caste Wise" link
            var doc = new HtmlDocument();
            doc.LoadHtml(indexHtml);
            var indexLinks = doc.DocumentNode.SelectNodes("//a");
            string catwiseLink = "";
            if (indexLinks != null)
            {
                foreach (var link in indexLinks)
                {
                    if (link.InnerText.Trim() == "Registration Caste Wise")
                    {
                        catwiseLink = nicBase + link.Attributes["href"]?.Value;
                        break;
                    }
                }
            }

            // Step 2: GET PoIndexFrame.aspx → PO session + find emuster/disabled/scst links
            _log.LogInformation("getcatwisejobcards: Step 2 — PoIndexFrame");
            var poUrl = $"{nicBase}Progofficer/PoIndexFrame.aspx?flag_debited=S&lflag=eng" +
                $"&District_Code={dist_code}&district_name={Uri.EscapeDataString(dist_name ?? "")}" +
                $"&state_name=KARNATAKA&state_Code=15" +
                $"&finyear={fin_year}&check=1" +
                $"&block_name={Uri.EscapeDataString(block_name ?? "")}&Block_Code={block_code}";

            var poCookies = new CookieContainer();
            using var poClient = CreateNicClient(poCookies);  // 60s, with cookies

            var poResp = await poClient.GetAsync(poUrl);
            var poHtml = await poResp.Content.ReadAsStringAsync();
            // Same pattern — read PO session from its CookieContainer
            string poSession = "";
            try
            {
                var nicUri = new Uri("https://nregastrep.nic.in/");
                var sc = poCookies.GetCookies(nicUri)["ASP.NET_SessionId"];
                poSession = sc?.Value ?? "";
            }
            catch { }

            // Find emuster, disabled, scst links from PO page
            doc = new HtmlDocument();
            doc.LoadHtml(poHtml);
            var poLinks = doc.DocumentNode.SelectNodes("//a");
            string mustrollLink = "", disabledLink = "", scstLink = "";
            if (poLinks != null)
            {
                foreach (var link in poLinks)
                {
                    var href = (link.Attributes["href"]?.Value ?? "").Replace("../", nicBase);
                    if (href.Contains("emuster_wagelist_rpt.aspx?"))
                        mustrollLink = href;
                    if (href.Contains("stdisabled.aspx?"))
                        disabledLink = href;
                    if (href.Contains("empstatusnewall_scst.aspx?"))
                        scstLink = href;
                    if (!string.IsNullOrEmpty(mustrollLink) && !string.IsNullOrEmpty(disabledLink) && !string.IsNullOrEmpty(scstLink))
                        break;
                }
            }

            _log.LogInformation("getcatwisejobcards: Links found — catwise:{C}, muster:{M}, disabled:{D}, scst:{S}",
                !string.IsNullOrEmpty(catwiseLink), !string.IsNullOrEmpty(mustrollLink),
                !string.IsNullOrEmpty(disabledLink), !string.IsNullOrEmpty(scstLink));

            // Step 3: Fetch all 4 data pages in parallel with session cookies
            string regcat = "", muster = "", disabled = "", scst = "";

            var tasks = new List<Task>();

            // Regcat (Registration Caste Wise) — uses index session
            if (!string.IsNullOrEmpty(catwiseLink))
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var req = new HttpRequestMessage(HttpMethod.Get, catwiseLink);
                        if (!string.IsNullOrEmpty(indexSession))
                            req.Headers.Add("Cookie", $"ASP.NET_SessionId={indexSession}");
                        using var c = CreateNicClient();
                        var r = await c.SendAsync(req);
                        var html = await r.Content.ReadAsStringAsync();
                        // Extract last row of skyblue table (matches original)
                        var d = new HtmlDocument();
                        d.LoadHtml(html);
                        var node = d.DocumentNode.SelectSingleNode("//table[@bgcolor='skyblue']/tr[last()]");
                        regcat = node?.OuterHtml ?? html;
                    }
                    catch (Exception ex) { _log.LogWarning("regcat fetch failed: {Err}", ex.Message); }
                }));
            }

            // Muster roll — uses PO session
            if (!string.IsNullOrEmpty(mustrollLink))
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var req = new HttpRequestMessage(HttpMethod.Get, mustrollLink);
                        if (!string.IsNullOrEmpty(poSession))
                            req.Headers.Add("Cookie", $"ASP.NET_SessionId={poSession}");
                        using var c = CreateNicClient();
                        muster = await (await c.SendAsync(req)).Content.ReadAsStringAsync();
                    }
                    catch (Exception ex) { _log.LogWarning("muster fetch failed: {Err}", ex.Message); }
                }));
            }

            // Disabled persons — uses PO session
            if (!string.IsNullOrEmpty(disabledLink))
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var req = new HttpRequestMessage(HttpMethod.Get, disabledLink);
                        if (!string.IsNullOrEmpty(poSession))
                            req.Headers.Add("Cookie", $"ASP.NET_SessionId={poSession}");
                        using var c = CreateNicClient();
                        disabled = await (await c.SendAsync(req)).Content.ReadAsStringAsync();
                    }
                    catch (Exception ex) { _log.LogWarning("disabled fetch failed: {Err}", ex.Message); }
                }));
            }

            // SCST employment — uses PO session
            if (!string.IsNullOrEmpty(scstLink))
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var req = new HttpRequestMessage(HttpMethod.Get, scstLink);
                        if (!string.IsNullOrEmpty(poSession))
                            req.Headers.Add("Cookie", $"ASP.NET_SessionId={poSession}");
                        using var c = CreateNicClient();
                        scst = await (await c.SendAsync(req)).Content.ReadAsStringAsync();
                    }
                    catch (Exception ex) { _log.LogWarning("scst fetch failed: {Err}", ex.Message); }
                }));
            }

            await Task.WhenAll(tasks);

            // Return $$-separated text matching original format
            var combined = $"{regcat}$${muster}$${disabled}$${scst}";
            _log.LogInformation("getcatwisejobcards: Combined response length={Len}", combined.Length);
            return Content(combined, "text/plain");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "getcatwisejobcards failed");
            return StatusCode(502, new { error = "Failed to fetch category-wise job cards." });
        }
    }

    // NOTE: /api/proxy/register3workmap and /api/proxy/register3paymentmap have been
    // moved to their own controllers (Register3WorkmapController.cs and
    // Register3PaymentMapController.cs) which implement the correct multi-stage
    // NIC crawl. The stubs that were here have been removed to avoid routing conflicts.

    // ── GET /api/proxy/cashbook ────────────────────────────────────
    // Used by cashbook.js v2.0 to load monthly cashbook data
    [HttpGet("cashbook")]
    public async Task<IActionResult> GetCashbook(
        [FromQuery] string panchayat_code,
        [FromQuery] string fin_year = "2025-2026",
        [FromQuery] string month = "01")
    {
        try
        {
            var html = await _nic.GetAsync(
                $"cashbook.aspx?state_code=15&panchayat_code={panchayat_code}" +
                $"&fin_year={fin_year}&month={month}&lflag=eng");
            return Ok(new { rawHtml = html });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "cashbook failed");
            return StatusCode(502, new { error = "Failed to fetch cashbook data." });
        }
    }

    // ── POST /api/proxy/musterrollmovement ───────────────────────────
    // Ported from MusterRollMovement.aspx.cs — multi-step NIC crawl that
    // navigates dynamic_muster_track.aspx dropdowns and returns date columns
    // for each NMR in the format: "DateFrom,MustClDate,wageDate,MustClDate,wageDate,FtoDate1,FtoDate2"
    [HttpPost("musterrollmovement")]
    public async Task<IActionResult> GetMusterRollMovement(
        [FromQuery] string distcode,
        [FromQuery] string gpcode,
        [FromQuery] string workcode,
        [FromQuery] string blockcode)
    {
        try
        {
            // Body arrives with contentType:"html" from the JS — read as raw string
            string body;
            using (var reader = new StreamReader(Request.Body))
                body = await reader.ReadToEndAsync();

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var nmrYears = root.GetProperty("NMRYears").EnumerateArray()
                .Select(e => e.GetString()!).ToList();
            var nmrs = root.GetProperty("NMRS").EnumerateArray()
                .Select(e => new {
                    NMRNo    = e.GetProperty("NMRNo").GetString()!,
                    DateFrom = e.GetProperty("DateFrom").GetString()!
                }).ToList();

            // Digest values for each financial year (same as original ASPX)
            var yearDigests = new Dictionary<string, string>
            {
                { "2025-2026", "rrqHsqSKol9m5Qs9mqXIvg" },
                { "2024-2025", "YF+8sXKOdXUOi3psKaS+nA" },
                { "2023-2024", "Y+NX28PGfSd8xSatAEQGbQ" },
                { "2022-2023", "ygoW2kfBkDzt0x81BI3S6g" },
                { "2021-2022", "ADBwMpShs9Ky/Z4IHpij0A" },
                { "2020-2021", "XVHWcm0cCjKEJf2GE2kgqQ" },
                { "2019-2020", "kiOJeLwfQzUj8T+aIHcqWg" }
            };

            var responseData = new Dictionary<string, string>();

            foreach (var year in nmrYears)
            {
                if (!yearDigests.TryGetValue(year, out var digest) || string.IsNullOrEmpty(digest))
                    continue;  // unknown year — skip

                // The original ASPX used a plain new HttpClient() which has UseCookies=true
                // by default. NIC's dynamic_muster_track.aspx sets an ASP.NET_SessionId
                // cookie on the initial GET — without it, every subsequent POST hits a fresh
                // session, the dropdown chain breaks, and the result table is empty.
                var yearCookies = new CookieContainer();
                using var client = CreateNicClient(yearCookies);

                // Build URL matching original exactly — do NOT Uri.EscapeDataString the
                // Digest: it contains Base64 '+' chars that the NIC server expects raw.
                var baseUrl =
                    $"https://nregastrep.nic.in/netnrega/dynamic_muster_track.aspx" +
                    $"?lflag=eng&state_code=15&fin_year={year}" +
                    $"&state_name=KARNATAKA&Digest={digest}";

                // Fresh HtmlDocument per year — avoids stale VIEWSTATE bleed between years.
                var htmlDoc = new HtmlDocument { OptionUseIdAttribute = true };

                // Step 1: GET initial page to seed session cookie + extract VIEWSTATE tokens
                var initResp = await client.GetAsync(baseUrl);
                htmlDoc.LoadHtml(await initResp.Content.ReadAsStringAsync());

                // Helper: URL-encode a hidden form field value from the current htmlDoc
                string EncField(string id) =>
                    Uri.EscapeDataString(htmlDoc.GetElementbyId(id)?.GetAttributeValue("value", "") ?? "");

                // Helper: build the standard VIEWSTATE/EVENTVALIDATION hidden fields
                string BaseHidden() =>
                    $"&__VIEWSTATE={EncField("__VIEWSTATE")}" +
                    $"&__VIEWSTATEGENERATOR={EncField("__VIEWSTATEGENERATOR")}" +
                    $"&__SCROLLPOSITIONX=0&__SCROLLPOSITIONY=0&__VIEWSTATEENCRYPTED=" +
                    $"&__EVENTVALIDATION={EncField("__EVENTVALIDATION")}";

                // Helper: POST a dropdown-change step; updates htmlDoc with the response HTML
                async Task PostStep(string eventTarget, string extraFields)
                {
                    var form = $"__EVENTTARGET={Uri.EscapeDataString(eventTarget)}&__EVENTARGUMENT=&__LASTFOCUS=" +
                               BaseHidden() + extraFields;
                    var resp = await client.PostAsync(baseUrl,
                        new StringContent(form, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded"));
                    htmlDoc.LoadHtml(await resp.Content.ReadAsStringAsync());
                }

                // Step 2: Select State (Karnataka = 15)
                await PostStep("ctl00$ContentPlaceHolder1$ddl_state",
                    "&ctl00%24ContentPlaceHolder1%24ddl_state=15" +
                    "&ctl00%24ContentPlaceHolder1%24TextBox1=&ctl00%24ContentPlaceHolder1%24TextBox2=" +
                    "&ctl00%24ContentPlaceHolder1%24Rbtn_pay=0");

                // Step 3: Select District
                await PostStep("ctl00$ContentPlaceHolder1$ddl_dist",
                    "&ctl00%24ContentPlaceHolder1%24ddl_state=15" +
                    $"&ctl00%24ContentPlaceHolder1%24ddl_dist={Uri.EscapeDataString(distcode)}" +
                    "&ctl00%24ContentPlaceHolder1%24TextBox1=&ctl00%24ContentPlaceHolder1%24TextBox2=" +
                    "&ctl00%24ContentPlaceHolder1%24Rbtn_pay=0");

                // Step 4: Select Block
                await PostStep("ctl00$ContentPlaceHolder1$ddl_blk",
                    "&ctl00%24ContentPlaceHolder1%24ddl_state=15" +
                    $"&ctl00%24ContentPlaceHolder1%24ddl_dist={Uri.EscapeDataString(distcode)}" +
                    $"&ctl00%24ContentPlaceHolder1%24ddl_blk={Uri.EscapeDataString(blockcode)}" +
                    "&ctl00%24ContentPlaceHolder1%24TextBox1=&ctl00%24ContentPlaceHolder1%24TextBox2=" +
                    "&ctl00%24ContentPlaceHolder1%24Rbtn_pay=0");

                // Step 5: Select Panchayat + submit
                await PostStep("",
                    "&ctl00%24ContentPlaceHolder1%24ddl_state=15" +
                    $"&ctl00%24ContentPlaceHolder1%24ddl_dist={Uri.EscapeDataString(distcode)}" +
                    $"&ctl00%24ContentPlaceHolder1%24ddl_blk={Uri.EscapeDataString(blockcode)}" +
                    $"&ctl00%24ContentPlaceHolder1%24ddl_pan={Uri.EscapeDataString(gpcode)}" +
                    "&ctl00%24ContentPlaceHolder1%24TextBox1=&ctl00%24ContentPlaceHolder1%24TextBox2=" +
                    "&ctl00%24ContentPlaceHolder1%24Rbtn_pay=0&ctl00%24ContentPlaceHolder1%24Button1=submit");

                // ── Find the data table ──────────────────────────────────────────────
                // Strategy 1 (original): 2nd <table> inside form's first <div>
                // Strategy 2 (fallback): any <table> in the page that has rows with 14+ <td>s
                // Strategy 3 (broadest): all <tr> in the page with 14+ <td>s
                var finalPageHtml = htmlDoc.DocumentNode.OuterHtml;
                _log.LogInformation("musterrollmovement year={year} html_len={len}", year, finalPageHtml.Length);

                List<HtmlNode> dataRows = new();

                // Strategy 1
                var strictTrs = htmlDoc.DocumentNode
                    .SelectNodes("//html[1]//body[1]//form[1]//div[1]//table[2]//tr");
                if (strictTrs != null)
                {
                    dataRows = strictTrs.Skip(2)
                        .Where(r => (r.SelectNodes("td")?.Count ?? 0) >= 14)
                        .ToList();
                    _log.LogInformation("musterrollmovement strategy1 total_trs={t} data_rows={d}",
                        strictTrs.Count, dataRows.Count);
                }

                // Strategy 2 — scan all tables
                if (dataRows.Count == 0)
                {
                    var allTables = htmlDoc.DocumentNode.SelectNodes("//table");
                    if (allTables != null)
                    {
                        foreach (var tbl in allTables)
                        {
                            var rows = tbl.SelectNodes(".//tr");
                            if (rows == null) continue;
                            var candidates = rows.Where(r => (r.SelectNodes("td")?.Count ?? 0) >= 14).ToList();
                            _log.LogInformation("musterrollmovement strategy2 table candidates={c}", candidates.Count);
                            if (candidates.Count > 0) { dataRows = candidates; break; }
                        }
                    }
                }

                // Strategy 3 — all <tr> anywhere with 14+ <td>
                if (dataRows.Count == 0)
                {
                    var allTrs = htmlDoc.DocumentNode.SelectNodes("//tr");
                    if (allTrs != null)
                        dataRows = allTrs.Where(r => (r.SelectNodes("td")?.Count ?? 0) >= 14).ToList();
                    _log.LogInformation("musterrollmovement strategy3 rows={c}", dataRows.Count);
                }

                if (dataRows.Count == 0)
                {
                    _log.LogWarning("musterrollmovement year={year}: no data rows found — page may be empty or structure changed", year);
                    continue;
                }

                // Log sample row so we can verify column layout
                var sampleCells = dataRows[0].SelectNodes("td");
                if (sampleCells != null)
                {
                    var sample = string.Join(" | ", sampleCells.Take(15).Select((c, idx) => $"[{idx}]={c.InnerText.Trim()}"));
                    _log.LogInformation("musterrollmovement sample_row: {row}", sample);
                }

                foreach (var nmr in nmrs)
                {
                    if (responseData.ContainsKey(nmr.NMRNo)) continue;

                    foreach (var row in dataRows)
                    {
                        var cells = row.SelectNodes("td");
                        if (cells == null || cells.Count < 14) continue;

                        // td[3]=cells[2] NMR No, td[6]=cells[5] work code
                        var cellNmr  = cells[2].InnerText.Trim();
                        var cellWork = cells[5].InnerText.Trim();
                        bool nmrMatch  = cellNmr == nmr.NMRNo ||
                                         cellNmr.TrimStart('0') == nmr.NMRNo.TrimStart('0');
                        bool workMatch = string.Equals(cellWork, workcode,
                                             StringComparison.OrdinalIgnoreCase);
                        if (!nmrMatch || !workMatch) continue;

                        try
                        {
                            var fmt     = "dd-MM-yyyy";
                            var culture = System.Globalization.CultureInfo.InvariantCulture;

                            // 0-indexed columns (same as original ASPX td[N] 1-indexed):
                            //   [6]  = td[7]  = Muster Roll Close date
                            //   [8]  = td[9]  = Wage/Payment date
                            //   [12] = td[13] = FTO 1st Signatory date
                            //   [13] = td[14] = FTO 2nd Signatory date
                            var mustCl   = DateTime.ParseExact(cells[6].InnerText.Trim(),  fmt, culture);
                            var wageDate = DateTime.ParseExact(cells[8].InnerText.Trim(),  fmt, culture);
                            var ftoDate1 = DateTime.ParseExact(cells[12].InnerText.Trim(), fmt, culture);
                            var ftoDate2 = DateTime.ParseExact(cells[13].InnerText.Trim(), fmt, culture);

                            responseData[nmr.NMRNo] =
                                nmr.DateFrom                    + "," +
                                mustCl.ToString("dd/MM/yyyy")   + "," +
                                wageDate.ToString("dd/MM/yyyy") + "," +
                                mustCl.ToString("dd/MM/yyyy")   + "," +
                                wageDate.ToString("dd/MM/yyyy") + "," +
                                ftoDate1.ToString("dd/MM/yyyy") + "," +
                                ftoDate2.ToString("dd/MM/yyyy");

                            _log.LogInformation("musterrollmovement matched NMR={nmr}", nmr.NMRNo);
                            break;
                        }
                        catch (Exception ex)
                        {
                            _log.LogWarning("musterrollmovement date parse failed nmr={nmr} row_cells={c}: {msg}",
                                nmr.NMRNo, cells.Count, ex.Message);
                        }
                    }
                }
            }

            // Mirrors old ASPX: Response.Write(JsonConvert.SerializeObject(responseData))
            // Old code had no explicit Content-Type so it defaulted to text/html.
            // MusterRollMovement.js does JSON.parse(s) — it expects a raw string, not
            // an already-parsed object. Returning text/html prevents jQuery auto-parsing.
            var json = System.Text.Json.JsonSerializer.Serialize(responseData);
            return Content(json, "text/html");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "musterrollmovement failed");
            return StatusCode(502, new { error = "Failed to fetch muster roll movement data." });
        }
    }

    // ParseWorkData removed — getworkdata now does multi-step crawl
    // and returns JSON dictionary directly (same format as original)
}

// ── Public proxy for panchayat code lookup (no auth needed on login page) ──
[Route("pullgpcodes")]
[ApiController]
public class GpCodesController : ControllerBase
{
    private readonly INicProxyService _nic;

    public GpCodesController(INicProxyService nic) => _nic = nic;

    [HttpGet]
    public async Task<IActionResult> Pull([FromQuery] string pull, [FromQuery] string code)
    {
        try
        {
            string url = pull switch
            {
                "district" => $"pullGPcode.aspx?pull=district&state_code=15&code={code}",
                "block" => $"pullGPcode.aspx?pull=block&district_code={code}",
                "panchayat" => $"pullGPcode.aspx?pull=panchayat&block_code={code}",
                _ => throw new ArgumentException("Invalid pull type")
            };

            var html = await _nic.GetAsync(url);
            return Content(html, "text/html");
        }
        catch
        {
            return Content("<html><body><label>Error</label></body></html>", "text/html");
        }
    }
}