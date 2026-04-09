using GpMnrega.DataLayer.Repositories;
using GpMnrega.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace GpMnrega.Web.Controllers;

/// <summary>
/// Handles email verification flow.
/// On successful verification, also triggers Google Translate
/// for regional names (Kannada) — same logic as original Emailverification.aspx.cs
/// </summary>
public class EmailVerificationController : Controller
{
    private readonly IGpUserRepository _gp;
    private readonly IGpCodeRepository _gpCode;
    private readonly IEmailService _email;
    private readonly ITranslationService _translate;
    private readonly ILogger<EmailVerificationController> _log;

    public EmailVerificationController(
        IGpUserRepository gp,
        IGpCodeRepository gpCode,
        IEmailService email,
        ITranslationService translate,
        ILogger<EmailVerificationController> log)
    {
        _gp = gp;
        _gpCode = gpCode;
        _email = email;
        _translate = translate;
        _log = log;
    }

    // GET /emailverification?email=...&h=...
    // GET /emailverification?email=... (just registered - show "check your email")
    [HttpGet("emailverification")]
    public async Task<IActionResult> Index(string? email, string? h, string? from)
    {
        if (!string.IsNullOrEmpty(h) && !string.IsNullOrEmpty(email))
        {
            // Verify the activation link
            var valid = await _gp.VerifyEmailAsync(email, h);
            if (valid)
            {
                await _gp.UpdateEmailActivationAsync(email);

                // Trigger regional name translation (same as original)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await TranslateRegionalNamesAsync(email);
                    }
                    catch (Exception ex)
                    {
                        _log.LogWarning(ex, "Regional name translation failed for {Email}", email);
                    }
                });

                // Sign out any stale session
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                ViewBag.Verified = true;
                ViewBag.Email = email;
                return View("~/Views/Auth/EmailVerification.cshtml");
            }
            else
            {
                ViewBag.Error = "Invalid or expired verification link. Please register again or contact support.";
                ViewBag.Email = email;
                return View("~/Views/Auth/EmailVerification.cshtml");
            }
        }
        else if (from == "master" && !string.IsNullOrEmpty(email))
        {
            // Came from the auth master redirect — resend verification email
            try
            {
                var activationCode = Guid.NewGuid().ToString();
                await _email.SendActivationEmailAsync(email, activationCode, Request.Host.Value);
                ViewBag.Email = email;
                ViewBag.Resent = true;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to resend activation email to {Email}", email);
                ViewBag.Error = "Failed to send email. Please try again later.";
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return View("~/Views/Auth/EmailVerification.cshtml");
        }
        else if (!string.IsNullOrEmpty(email))
        {
            // Just registered — show "check your email" state
            ViewBag.Email = email;
            return View("~/Views/Auth/EmailVerification.cshtml");
        }

        return Redirect("/login");
    }

    /// <summary>
    /// Translate GP, block, district, and constituency names to Kannada
    /// using Google Cloud Translate — mirrors original Emailverification.aspx.cs logic.
    /// </summary>
    private async Task TranslateRegionalNamesAsync(string email)
    {
        var data = await _gpCode.GetOfficialLanguageAsync(email);
        if (data == null) return;

        var langCode = data.LanguageCode?.Trim();
        if (string.IsNullOrEmpty(langCode)) langCode = "kn";

        // Panchayat name
        if (string.IsNullOrEmpty(data.PanchayatNameRegional) &&
            !string.IsNullOrEmpty(data.PanchyatName))
        {
            var translated = await _translate.TranslateAsync(
                ToTitleCase(data.PanchyatName), "en", langCode);
            if (!string.IsNullOrEmpty(translated))
                await _gpCode.UpdatePanchayatRegionalNameAsync(data.PanchyatCode, translated);
        }

        // Vidhan Sabha + Lok Sabha
        if (string.IsNullOrEmpty(data.VidhanSabhaRegional) &&
            !string.IsNullOrEmpty(data.VidhanSabha))
        {
            var translatedVidhan = await _translate.TranslateAsync(
                ToTitleCase(data.VidhanSabha), "en", langCode);
            var translatedLok = await _translate.TranslateAsync(
                ToTitleCase(data.LokSabha), "en", langCode);
            if (!string.IsNullOrEmpty(translatedVidhan))
                await _gpCode.UpdateVidLokRegionalNameAsync(email,
                    translatedLok ?? "", translatedVidhan);
        }

        // Block / Taluk name
        if (string.IsNullOrEmpty(data.TalukNameRegional) &&
            !string.IsNullOrEmpty(data.TalukName))
        {
            var translated = await _translate.TranslateAsync(
                ToTitleCase(data.TalukName), "en", langCode);
            if (!string.IsNullOrEmpty(translated))
                await _gpCode.UpdateBlockRegionalNameAsync(data.TalukCode, translated);
        }

        // District name
        if (string.IsNullOrEmpty(data.DistrictNameRegional) &&
            !string.IsNullOrEmpty(data.DistrictName))
        {
            var translated = await _translate.TranslateAsync(
                ToTitleCase(data.DistrictName), "en", langCode);
            if (!string.IsNullOrEmpty(translated))
                await _gpCode.UpdateDistrictRegionalNameAsync(data.DistrictCode, translated);
        }
    }

    /// <summary>
    /// Title-cases a string: "BIJAPUR NORTH" → "Bijapur North"
    /// Mirrors the word-by-word title casing in original code.
    /// </summary>
    private static string ToTitleCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;
        return string.Join(" ", input.Split(' ')
            .Select(w => w.Length > 0
                ? char.ToUpper(w[0]) + w.Substring(1).ToLower()
                : w));
    }
}
