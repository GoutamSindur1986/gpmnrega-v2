using System.IdentityModel.Tokens.Jwt;
using Microsoft.JSInterop;

namespace GpMnrega.Wasm.Services;

/// <summary>
/// Reads the SubToken JWT cookie (set by server on login).
/// Determines whether user is on trial (watermark = true) or subscribed.
/// This logic runs in WASM — not visible as plain JS source.
/// </summary>
public interface ISubscriptionService
{
    Task<bool> IsSubscribedAsync();
    Task<string> GetUserEmailAsync();
    Task<string> GetUserTypeAsync();
}

public class SubscriptionService : ISubscriptionService
{
    private readonly IJSRuntime _js;
    private JwtSecurityToken? _token;

    public SubscriptionService(IJSRuntime js) => _js = js;

    private async Task<JwtSecurityToken?> GetTokenAsync()
    {
        if (_token != null) return _token;

        try
        {
            // Read SubToken cookie — this cookie is NOT HttpOnly so WASM can read it
            var cookieStr = await _js.InvokeAsync<string>("eval",
                "document.cookie.split('; ').find(r=>r.startsWith('SubToken='))?.split('=')[1] || ''");

            if (string.IsNullOrWhiteSpace(cookieStr)) return null;

            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(cookieStr)) return null;

            // We only DECODE (not verify signature) in WASM.
            // Signature verification happens server-side when the cookie was issued.
            // An attacker who modifies the JWT will get an invalid token on next API call.
            _token = handler.ReadJwtToken(cookieStr);
            return _token;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> IsSubscribedAsync()
    {
        var token = await GetTokenAsync();
        if (token == null) return false;
        var claim = token.Claims.FirstOrDefault(c => c.Type == "subscribed");
        return claim?.Value == "true";
    }

    public async Task<string> GetUserEmailAsync()
    {
        var token = await GetTokenAsync();
        return token?.Claims.FirstOrDefault(c =>
            c.Type == "email" ||
            c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")
            ?.Value ?? "";
    }

    public async Task<string> GetUserTypeAsync()
    {
        var token = await GetTokenAsync();
        return token?.Claims.FirstOrDefault(c => c.Type == "userType")?.Value ?? "GP";
    }
}

/// <summary>
/// Calls /api/proxy/* on our server — which proxies to NIC.
/// WASM calls our own domain → no CORS.
/// Server calls NIC → server-to-server, no CORS.
/// </summary>
public interface INicDataService
{
    Task<string> GetWorkDataAsync(string districtCode, string blockCode, string panchayatCode, string finYear);
    Task<string> GetNmrDataAsync(string workCode, string finYear);
    Task<string> GetWageListAsync(string nmrLink);
    Task<string> GetFtoDetailsAsync(string workCode, string finYear);
    Task<string> GetForm8DataAsync(string nmrLink);
    Task<string> GetAgencyWorkDataAsync(string blockCode, string finYear, string agency);
}

public class NicDataService : INicDataService
{
    private readonly HttpClient _http;

    public NicDataService(HttpClient http) => _http = http;

    public async Task<string> GetWorkDataAsync(string districtCode, string blockCode,
        string panchayatCode, string finYear)
    {
        var resp = await _http.GetAsync(
            $"/api/proxy/getworkdata?district_code={districtCode}" +
            $"&block_code={blockCode}&panchayat_code={panchayatCode}&fin_year={finYear}");
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStringAsync();
    }

    public async Task<string> GetNmrDataAsync(string workCode, string finYear)
    {
        var resp = await _http.GetAsync(
            $"/api/proxy/getnmrdata?work_code={Uri.EscapeDataString(workCode)}&fin_year={finYear}");
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStringAsync();
    }

    public async Task<string> GetWageListAsync(string nmrLink)
    {
        var resp = await _http.GetAsync(
            $"/api/proxy/getwagelist?nmr_link={Uri.EscapeDataString(nmrLink)}");
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStringAsync();
    }

    public async Task<string> GetFtoDetailsAsync(string workCode, string finYear)
    {
        var resp = await _http.GetAsync(
            $"/api/proxy/getftodetails?work_code={Uri.EscapeDataString(workCode)}&fin_year={finYear}");
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStringAsync();
    }

    public async Task<string> GetForm8DataAsync(string nmrLink)
    {
        var resp = await _http.GetAsync(
            $"/api/proxy/getform8data?nmr_link={Uri.EscapeDataString(nmrLink)}");
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStringAsync();
    }

    public async Task<string> GetAgencyWorkDataAsync(string blockCode,
        string finYear, string agency)
    {
        var resp = await _http.GetAsync(
            $"/api/proxy/getagencyworkdata?block_code={blockCode}" +
            $"&fin_year={finYear}&agency={Uri.EscapeDataString(agency)}");
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStringAsync();
    }
}
