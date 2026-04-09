using GpMnrega.DataLayer.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using System.Text;

namespace GpMnrega.Web.Controllers;

[Authorize]
[Route("api/payment")]
[ApiController]
public class PaymentController : ControllerBase
{
    private readonly IGpUserRepository _gp;
    private readonly IDeptUserRepository _dept;
    private readonly IConfiguration _cfg;
    private readonly ILogger<PaymentController> _log;

    public PaymentController(IGpUserRepository gp, IDeptUserRepository dept,
        IConfiguration cfg, ILogger<PaymentController> log)
    {
        _gp = gp;
        _dept = dept;
        _cfg = cfg;
        _log = log;
    }

    // POST /api/payment/initiate
    // Calls UPI gateway (same pattern as original PaySub.aspx btn_Subscribe_Click)
    [HttpPost("initiate")]
    public async Task<IActionResult> Initiate([FromBody] PaymentInitRequest req)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
        var name = User.FindFirst(ClaimTypes.Name)?.Value ?? "";
        var userType = User.FindFirst("UserType")?.Value ?? "GP";

        try
        {
            var gatewayUrl = _cfg["Payment:UpiGatewayUrl"];
            if (string.IsNullOrEmpty(gatewayUrl))
            {
                // Payment gateway not configured — return a placeholder response
                _log.LogWarning("Payment gateway URL not configured in appsettings.json");
                return BadRequest(new { error = "Payment gateway not configured. Please contact support." });
            }

            var phone = await GetUserPhoneAsync(email, userType);

            var payload = new
            {
                name,
                email,
                phone,
                amount = req.Amount,
                description = $"GP MNREGA {req.PlanType} subscription",
                transaction_id = Guid.NewGuid().ToString(),
                redirect_url = $"https://{Request.Host}/api/payment/response"
            };

            using var client = new HttpClient();
            var content = new StringContent(JsonSerializer.Serialize(payload),
                Encoding.UTF8, "application/json");
            var resp = await client.PostAsync(gatewayUrl, content);
            var json = await resp.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var status = doc.RootElement.GetProperty("status").GetString();
            if (status == "True")
            {
                var paymentUrl = doc.RootElement
                    .GetProperty("data").GetProperty("payment_url").GetString();
                return Ok(new { paymentUrl });
            }

            return BadRequest(new { error = "Payment gateway returned an error. Please try again." });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Payment initiation failed for {Email}", email);
            return StatusCode(500, new { error = "Payment service unavailable. Please try again later." });
        }
    }

    // GET /api/payment/response (redirect from payment gateway)
    [HttpGet("response")]
    [AllowAnonymous]
    public async Task<IActionResult> Response(
        [FromQuery] string? payment_id,
        [FromQuery] string? payment_request_id,
        [FromQuery] string? payment_status,
        [FromQuery] string? amount)
    {
        try
        {
            if (payment_status?.ToLower() == "credit" && !string.IsNullOrEmpty(payment_id))
            {
                // Payment successful — update subscription
                var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
                var userType = User.FindFirst("UserType")?.Value ?? "GP";
                decimal amt = decimal.TryParse(amount, out var a) ? a : 0;

                if (userType == "GP")
                    await _gp.UpdateSubscriptionAsync(email, payment_id,
                        payment_request_id ?? "", payment_status, amt);
                else
                    await _dept.UpdateSubscriptionAsync(email, payment_id,
                        payment_request_id ?? "", payment_status, amt);

                return Redirect("/Auth/Subscription?success=1");
            }

            return Redirect("/Auth/Subscription?failed=1");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Payment response processing failed");
            return Redirect("/Auth/Subscription?error=1");
        }
    }

    // POST /api/auth/forgot-password (used by PasswordReset view)
    [HttpPost("/api/auth/forgot-password")]
    [AllowAnonymous]
    public IActionResult ForgotPassword([FromForm] string email)
    {
        // In production: generate token, store it, send email with reset link
        // For now return success message always (prevents email enumeration)
        return Ok(new
        {
            message = "If that email is registered, a reset link has been sent. Please check your inbox."
        });
    }

    private async Task<string> GetUserPhoneAsync(string email, string userType)
    {
        try
        {
            if (userType == "GP")
            {
                var user = await _gp.GetSubscriptionAsync(email);
                return ""; // Phone not in model — add if needed
            }
            return "";
        }
        catch { return ""; }
    }
}

public class PaymentInitRequest
{
    public string PlanType { get; set; } = "";
    public decimal Amount { get; set; }
}
