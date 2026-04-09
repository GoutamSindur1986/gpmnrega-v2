using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace GpMnrega.Web.Controllers;

/// <summary>
/// Handles geotag data retrieval by proxying to the Bhuvan API.
/// Replaces the original geotag.aspx / geotag.aspx.cs WebForms template.
/// Kept as a separate file because ProxyController.cs is already large.
/// </summary>
[Authorize]
[Route("api/proxy")]
[ApiController]
public class GeotagController : ControllerBase
{
    private readonly ILogger<GeotagController> _log;

    private const string BhuvanUrl =
        "https://bhuvan-app2.nrsc.gov.in/mgnrega/nrega_dashboard_phase2/php/reports/accepted_geotags.php";

    public GeotagController(ILogger<GeotagController> log)
    {
        _log = log;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static HttpClient CreateBhuvanClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");
        client.Timeout = TimeSpan.FromSeconds(60);
        return client;
    }

    /// <summary>Fetches a remote image URL and returns a data-URI string, or "" on failure.</summary>
    private static async Task<string> FetchImageBase64Async(HttpClient client, string url)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url)) return "";
            var bytes = await client.GetByteArrayAsync(url).ConfigureAwait(false);
            return "data:image/jpeg;base64," + Convert.ToBase64String(bytes);
        }
        catch
        {
            return "";
        }
    }

    // ── GET /api/proxy/geotag ─────────────────────────────────────────────────
    // Called by loadAndGenerateGeotag() in gphome.js.
    // Mirrors original geotag.aspx.cs:
    //   • POSTs to Bhuvan for stages 1–3
    //   • Finds the entry whose workcode matches the requested work_code
    //   • Fetches path1/path2 photos and converts them to base64 data-URIs
    //   • Returns JSON that generategeotag.js reads directly (no HTML parsing)
    [HttpGet("geotag")]
    public async Task<IActionResult> GetGeotag(
        [FromQuery] string work_code,
        [FromQuery] string district_code   = "",
        [FromQuery] string block_code      = "",
        [FromQuery] string panchayat_code  = "",
        [FromQuery] string block_name      = "",
        [FromQuery] string panchayat_name  = "",
        [FromQuery] string district_name   = "",
        [FromQuery] string fin_year        = "2025-2026",
        [FromQuery] string work_name       = "")
    {
        // Mutable per-stage slots, filled as we process each stage
        var lat          = "";
        var lon          = "";
        var dateCreation = "";
        var stageImgs    = new (string img1, string img2)[3];  // index 0=stage1, 1=stage2, 2=stage3

        using var client = CreateBhuvanClient();
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        for (int stage = 1; stage <= 3; stage++)
        {
            // POST body exactly as original geotag.aspx.cs
            var postBody =
                $"username=unauthourized" +
                $"&sub_category_id=All" +
                $"&state_code=15" +
                $"&start_date=2019-03-31" +
                $"&stage={stage}" +
                $"&panchayat_code={Uri.EscapeDataString(panchayat_code)}" +
                $"&financial_year=All" +
                $"&end_date={today}" +
                $"&district_code={Uri.EscapeDataString(district_code)}" +
                $"&category_id=All" +
                $"&block_code={Uri.EscapeDataString(block_code)}" +
                $"&accuracy=0";

            try
            {
                using var content  = new StringContent(postBody, Encoding.UTF8, "application/x-www-form-urlencoded");
                using var response = await client.PostAsync(BhuvanUrl, content).ConfigureAwait(false);
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.ValueKind != JsonValueKind.Array) continue;

                // Find the array item whose "workcode" matches (case-insensitive, trimmed)
                // Mirrors original: item.GetValue("workcode").ToString().Trim().ToLower() == workcode.Trim().ToLower()
                JsonElement? match = null;
                foreach (var item in root.EnumerateArray())
                {
                    if (item.TryGetProperty("workcode", out var wc) &&
                        wc.GetString()?.Trim().ToLower() == work_code.Trim().ToLower())
                    {
                        match = item;
                        break;
                    }
                }

                if (match is null) continue;
                var m = match.Value;

                // lat/lon/date — same for every stage, only read once
                if (string.IsNullOrEmpty(lat))
                {
                    lat = m.TryGetProperty("lat", out var latEl) ? latEl.GetString() ?? "" : "";
                    lon = m.TryGetProperty("lon", out var lonEl) ? lonEl.GetString() ?? "" : "";

                    if (m.TryGetProperty("creationtime", out var ctEl))
                    {
                        var rawDate = ctEl.GetString() ?? "";
                        var datePart = rawDate.Split(' ')[0];
                        dateCreation = DateTime.TryParse(datePart, out var parsed)
                            ? parsed.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
                            : datePart;
                    }
                }

                // Fetch the two photos for this stage in parallel
                var path1 = m.TryGetProperty("path1", out var p1El) ? p1El.GetString() ?? "" : "";
                var path2 = m.TryGetProperty("path2", out var p2El) ? p2El.GetString() ?? "" : "";

                var results = await Task.WhenAll(
                    FetchImageBase64Async(client, path1),
                    FetchImageBase64Async(client, path2)).ConfigureAwait(false);

                stageImgs[stage - 1] = (results[0], results[1]);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Bhuvan geotag stage {Stage} failed for workcode={Work}", stage, work_code);
                // Continue — a missing stage is acceptable; the JS falls back to a blank image
            }
        }

        return Ok(new
        {
            panch        = panchayat_name,
            block        = block_name,
            dist         = district_name,
            workName     = work_name,
            workCode     = work_code,
            fin          = fin_year,
            lat,
            lon,
            dateCreation,
            stages = new[]
            {
                new { img1 = stageImgs[0].img1 ?? "", img2 = stageImgs[0].img2 ?? "" },
                new { img1 = stageImgs[1].img1 ?? "", img2 = stageImgs[1].img2 ?? "" },
                new { img1 = stageImgs[2].img1 ?? "", img2 = stageImgs[2].img2 ?? "" }
            }
        });
    }
}
