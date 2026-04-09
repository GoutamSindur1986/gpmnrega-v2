using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace GpMnrega.Web.Services;

// ── Email Service ─────────────────────────────────────────────────────────────

public interface IEmailService
{
    Task SendActivationEmailAsync(string toEmail, string activationCode, string siteAuthority);
    Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string siteAuthority);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _cfg;

    public EmailService(IConfiguration cfg) => _cfg = cfg;

    public async Task SendActivationEmailAsync(string toEmail, string activationCode, string siteAuthority)
    {
        var link = $"https://{siteAuthority}/emailverification?email={Uri.EscapeDataString(toEmail)}&h={activationCode.Trim()}";
        var body = $@"
            <div style='font-family:Poppins,sans-serif;max-width:480px;margin:40px auto;padding:32px;border:1px solid #e5e7eb;border-radius:12px'>
              <h2 style='color:#1e3a5f;margin-bottom:8px'>Verify your email</h2>
              <p style='color:#6b7280;margin-bottom:24px'>Click the button below to activate your GP MNREGA account.</p>
              <a href='{link}' style='background:#4F46E5;color:#fff;padding:12px 28px;border-radius:8px;text-decoration:none;font-weight:600;display:inline-block'>
                Verify Email
              </a>
              <p style='color:#9ca3af;font-size:12px;margin-top:24px'>If you didn't create this account, ignore this email.</p>
            </div>";

        await SendAsync(toEmail, "Verify your GP MNREGA email", body);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string siteAuthority)
    {
        var link = $"https://{siteAuthority}/passwordreset?email={Uri.EscapeDataString(toEmail)}&token={resetToken}";
        var body = $@"
            <div style='font-family:Poppins,sans-serif;max-width:480px;margin:40px auto;padding:32px;border:1px solid #e5e7eb;border-radius:12px'>
              <h2 style='color:#1e3a5f'>Reset your password</h2>
              <p style='color:#6b7280;margin-bottom:24px'>Click below to reset your password. Link expires in 1 hour.</p>
              <a href='{link}' style='background:#4F46E5;color:#fff;padding:12px 28px;border-radius:8px;text-decoration:none;font-weight:600;display:inline-block'>
                Reset Password
              </a>
            </div>";

        await SendAsync(toEmail, "Reset your GP MNREGA password", body);
    }

    private async Task SendAsync(string to, string subject, string htmlBody)
    {
        var smtp = _cfg.GetSection("Smtp");
        using var client = new SmtpClient(smtp["Host"], smtp.GetValue<int>("Port", 25))
        {
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new System.Net.NetworkCredential(smtp["FromEmail"], smtp["Password"])
        };

        var msg = new MailMessage(
            new MailAddress(smtp["FromEmail"]!, smtp["FromName"]),
            new MailAddress(to))
        {
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        await client.SendMailAsync(msg);
    }
}

// ── Cookie Obfuscation Service ────────────────────────────────────────────────
// Simple XOR + Base64 — not cryptographic, just stops casual reading.
// The real security is HttpOnly on the auth cookie (unreadable by JS at all).

public interface ICookieObfuscationService
{
    string Obfuscate(string value);
    string Deobfuscate(string value);
}

public class CookieObfuscationService : ICookieObfuscationService
{
    // 16-byte key — keep in appsettings or environment variable in production
    private readonly byte[] _key;

    public CookieObfuscationService(IConfiguration cfg)
    {
        var keyStr = cfg["Auth:ObfuscationKey"] ?? "S3KN_GP_XOR_KEY!";
        _key = Encoding.UTF8.GetBytes(keyStr.PadRight(16).Substring(0, 16));
    }

    public string Obfuscate(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var xored = XorBytes(bytes);
        return Convert.ToBase64String(xored);
    }

    public string Deobfuscate(string value)
    {
        try
        {
            var bytes = Convert.FromBase64String(value);
            var xored = XorBytes(bytes);
            return Encoding.UTF8.GetString(xored);
        }
        catch
        {
            return "";
        }
    }

    private byte[] XorBytes(byte[] input)
    {
        var result = new byte[input.Length];
        for (int i = 0; i < input.Length; i++)
            result[i] = (byte)(input[i] ^ _key[i % _key.Length]);
        return result;
    }
}

// ── NIC Proxy Service ─────────────────────────────────────────────────────────
// Server-side proxy to nregastrep.nic.in. WASM calls /api/proxy/* on our server.
// Our server forwards to NIC (server-to-server, no CORS).

public interface INicProxyService
{
    Task<string> GetAsync(string nicRelativeUrl);
    Task<string> PostAsync(string nicRelativeUrl, HttpContent content);
}

public class NicProxyService : INicProxyService
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly ILogger<NicProxyService> _log;

    public NicProxyService(HttpClient http, IConfiguration cfg, ILogger<NicProxyService> log)
    {
        _http = http;
        _baseUrl = cfg["NicProxy:BaseUrl"] ?? "https://nregastrep.nic.in/netnrega/";
        _log = log;

        // Mimic browser headers so NIC server doesn't reject the request
        _http.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        _http.DefaultRequestHeaders.Add("Accept",
            "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
    }

    public async Task<string> GetAsync(string nicRelativeUrl)
    {
        var url = _baseUrl + nicRelativeUrl;
        _log.LogInformation("NIC proxy GET: {Url}", url);
        var resp = await _http.GetAsync(url);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStringAsync();
    }

    public async Task<string> PostAsync(string nicRelativeUrl, HttpContent content)
    {
        var url = _baseUrl + nicRelativeUrl;
        _log.LogInformation("NIC proxy POST: {Url}", url);
        var resp = await _http.PostAsync(url, content);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStringAsync();
    }
}
