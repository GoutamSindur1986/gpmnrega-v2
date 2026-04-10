using GpMnrega.DataLayer.Repositories;
using GpMnrega.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GpMnrega.Web.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _auth;
    private readonly IEmailService _email;
    private readonly IGpUserRepository _gp;
    private readonly IDeptUserRepository _dept;
    private readonly IAdsRepository _ads;

    public AuthController(IAuthService auth, IEmailService email,
        IGpUserRepository gp, IDeptUserRepository dept, IAdsRepository ads)
    {
        _auth = auth;
        _email = email;
        _gp = gp;
        _dept = dept;
        _ads = ads;
    }

    // GET /login
    [HttpGet("login")]
    public async Task<IActionResult> Login(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userType = User.FindFirst("UserType")?.Value;
            return Redirect(userType == "Dept" ? "/AuthAgency/DeptHome" : "/Auth/Home");
        }

        ViewBag.Ads = await _ads.GetLoginPageAdsAsync();
        ViewBag.ReturnUrl = returnUrl;
        ViewBag.UserType = "GP";
        return View();
    }

    // POST /login — GP login
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl)
    {
        var result = await _auth.LoginGpAsync(email, password);

        if (!result.Success)
        {
            ViewBag.Ads = await _ads.GetLoginPageAdsAsync();
            ViewBag.Error = result.Error;
            ViewBag.UserType = "GP";
            return View();
        }

        if (!result.IsActivated)
            return RedirectToAction("EmailVerification", new { email = result.UserEmail });

        // Sign in with cookie
        var principal = _auth.BuildClaimsPrincipal(result);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            principal, new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
            });

        // Store subscription JWT for WASM to read (not HttpOnly so WASM can access)
        var subToken = _auth.IssueSubscriptionToken(result.UserEmail, result.IsSubscribed, "GP");
        Response.Cookies.Append("SubToken", subToken, new CookieOptions
        {
            HttpOnly = false,        // WASM needs to read this
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddHours(8)
        });

        // Trial users (IsSubscribed=false) are allowed into Home — their PDFs
        // will carry the "TRIAL VERSION" watermark (EvoPDF: ProxyController.ConverterWatermark;
        // pdfMake: generateNewPdf in gphome.js reads IsSubscribed from page config).
        // PaySub page is still reachable via nav for them to upgrade.
        return Redirect(returnUrl ?? "/Auth/Home");
    }

    // GET /deptlogin
    [HttpGet("deptlogin")]
    public async Task<IActionResult> DeptLogin(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userType = User.FindFirst("UserType")?.Value;
            return Redirect(userType == "Dept" ? "/AuthAgency/DeptHome" : "/Auth/Home");
        }

        ViewBag.Ads = await _ads.GetLoginPageAdsAsync();
        ViewBag.ReturnUrl = returnUrl;
        ViewBag.UserType = "Dept";
        return View("Login");   // Same view, different tab active
    }

    // POST /deptlogin
    [HttpPost("deptlogin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeptLogin(string email, string password, string? returnUrl)
    {
        var result = await _auth.LoginDeptAsync(email, password);

        if (!result.Success)
        {
            ViewBag.Ads = await _ads.GetLoginPageAdsAsync();
            ViewBag.Error = result.Error;
            ViewBag.UserType = "Dept";
            return View("Login");
        }

        if (!result.IsActivated)
            return RedirectToAction("EmailVerification", new { email = result.UserEmail });

        var principal = _auth.BuildClaimsPrincipal(result);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            principal, new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
            });

        var subToken = _auth.IssueSubscriptionToken(result.UserEmail, result.IsSubscribed, "Dept");
        Response.Cookies.Append("SubToken", subToken, new CookieOptions
        {
            HttpOnly = false,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddHours(8)
        });

        // Same trial policy as GP: non-subscribers go to DeptHome (with watermark),
        // not hard-blocked at PaySubDept.
        return Redirect(returnUrl ?? "/AuthAgency/DeptHome");
    }

    // POST /logout
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        Response.Cookies.Delete("SubToken");
        return Redirect("/login");
    }

    // GET /logout (for nav link fallback)
    [HttpGet("logout")]
    public async Task<IActionResult> LogoutGet()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        Response.Cookies.Delete("SubToken");
        return Redirect("/login");
    }

    // GET /emailverification
    [HttpGet("emailverification")]
    public async Task<IActionResult> EmailVerification(string email, string? h, string? from)
    {
        if (!string.IsNullOrEmpty(h))
        {
            var ok = await _gp.VerifyEmailAsync(email, h);
            if (ok)
            {
                await _gp.UpdateEmailActivationAsync(email);
                ViewBag.Verified = true;
            }
            else
            {
                ViewBag.Error = "Invalid or expired verification link.";
            }
        }
        ViewBag.Email = email;
        return View();
    }

    // GET /register (GP)
    [HttpGet("register")]
    public async Task<IActionResult> Register()
    {
        ViewBag.Ads = await _ads.GetLoginPageAdsAsync();
        ViewBag.UserType = "GP";
        return View("Login");
    }

    // POST /api/auth/register-gp (called by login page form via fetch)
    [HttpPost("/api/auth/register-gp")]
    public async Task<IActionResult> RegisterGp([FromForm] GpRegisterModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { error = "Please fill all required fields." });

        var activationCode = Guid.NewGuid().ToString();
        var (success, error) = await _auth.RegisterGpAsync(
            model.UserName, model.Email, model.Password,
            model.PanchayatCode, model.VidhanSabha, model.LokSabha,
            model.Phone, activationCode);

        if (!success) return BadRequest(new { error });

        try
        {
            await _email.SendActivationEmailAsync(model.Email, activationCode,
                Request.Host.Value);
        }
        catch (Exception ex)
        {
            // Log but don't fail registration if email fails
            HttpContext.RequestServices.GetRequiredService<ILogger<AuthController>>()
                .LogError(ex, "Failed to send activation email to {Email}", model.Email);
        }

        return Ok(new { message = "Registration successful! Please check your email to activate your account." });
    }

    // POST /api/auth/register-dept
    [HttpPost("/api/auth/register-dept")]
    public async Task<IActionResult> RegisterDept([FromForm] DeptRegisterModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { error = "Please fill all required fields." });

        var (success, error) = await _auth.RegisterDeptAsync(
            model.UserName, model.Email, model.Password,
            model.BlockCode, model.Agency, model.Phone);

        if (!success) return BadRequest(new { error });
        return Ok(new { message = "Registration successful! Please check your email." });
    }

    // GET /api/auth/status — lightweight endpoint for WASM to check auth
    [HttpGet("/api/auth/status")]
    public IActionResult Status()
    {
        if (!User.Identity?.IsAuthenticated == true)
            return Unauthorized();

        return Ok(new
        {
            isAuthenticated = true,
            userType = User.FindFirst("UserType")?.Value,
            email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
            isSubscribed = User.FindFirst("IsSubscribed")?.Value == "true"
        });
    }

    // GET /passwordreset
    [HttpGet("passwordreset")]
    public IActionResult PasswordReset(string? email, string? token)
    {
        ViewBag.Email = email;
        ViewBag.Token = token;
        return View();
    }

    // POST /api/auth/reset-password
    [HttpPost("/api/auth/reset-password")]
    public async Task<IActionResult> ResetPassword([FromForm] string email,
        [FromForm] string newPassword)
    {
        // In production add token validation here
        var hash = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes(newPassword));
        var ok = await _gp.UpdatePasswordAsync(email, hash);
        if (!ok) return BadRequest(new { error = "Password reset failed." });
        return Ok(new { message = "Password updated. Please login." });
    }
}

// ── View Models ───────────────────────────────────────────────────────────────

public class GpRegisterModel
{
    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string PanchayatCode { get; set; } = "";
    public string VidhanSabha { get; set; } = "";
    public string LokSabha { get; set; } = "";
    public string Phone { get; set; } = "";
}

public class DeptRegisterModel
{
    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string BlockCode { get; set; } = "";
    public string Agency { get; set; } = "";
    public string Phone { get; set; } = "";
}
