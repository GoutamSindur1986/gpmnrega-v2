using GpMnrega.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace GpMnrega.Web.Controllers;

[Route("api/contact")]
[ApiController]
public class ContactController : ControllerBase
{
    private readonly IEmailService _email;
    private readonly ILogger<ContactController> _log;

    public ContactController(IEmailService email, ILogger<ContactController> log)
    {
        _email = email;
        _log = log;
    }

    // POST /api/contact/send
    [HttpPost("send")]
    public async Task<IActionResult> Send(
        [FromForm] string name,
        [FromForm] string email,
        [FromForm] string phone,
        [FromForm] string? organisation,
        [FromForm] string queryType,
        [FromForm] string message)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(message))
            return BadRequest(new { error = "Please fill all required fields." });

        try
        {
            // Send notification to admin
            await SendAdminNotificationAsync(name, email, phone, organisation, queryType, message);

            // Send acknowledgement to user
            await SendUserAcknowledgementAsync(name, email, queryType);

            return Ok(new { message = "Message sent successfully." });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Contact form submission failed from {Email}", email);
            return StatusCode(500, new { error = "Failed to send message. Please email us directly at admin@gpmnrega.com" });
        }
    }

    private async Task SendAdminNotificationAsync(string name, string email, string phone,
        string? org, string queryType, string message)
    {
        // Re-use EmailService infrastructure — send to admin inbox
        var body = $@"
        <div style='font-family:Poppins,sans-serif;max-width:600px;margin:0 auto;padding:24px'>
          <h2 style='color:#1e1b6e;border-bottom:2px solid #4f46e5;padding-bottom:12px'>
            New Contact Form Submission
          </h2>
          <table style='width:100%;border-collapse:collapse;font-size:14px'>
            <tr><td style='padding:8px;font-weight:600;color:#374151;width:140px'>Name</td><td style='padding:8px;color:#64748b'>{System.Net.WebUtility.HtmlEncode(name)}</td></tr>
            <tr style='background:#f8faff'><td style='padding:8px;font-weight:600;color:#374151'>Email</td><td style='padding:8px'><a href='mailto:{email}'>{email}</a></td></tr>
            <tr><td style='padding:8px;font-weight:600;color:#374151'>Phone</td><td style='padding:8px;color:#64748b'>{System.Net.WebUtility.HtmlEncode(phone)}</td></tr>
            <tr style='background:#f8faff'><td style='padding:8px;font-weight:600;color:#374151'>Organisation</td><td style='padding:8px;color:#64748b'>{System.Net.WebUtility.HtmlEncode(org ?? "—")}</td></tr>
            <tr><td style='padding:8px;font-weight:600;color:#374151'>Query Type</td><td style='padding:8px;color:#64748b'>{System.Net.WebUtility.HtmlEncode(queryType)}</td></tr>
          </table>
          <div style='margin-top:16px;padding:16px;background:#f1f5f9;border-radius:8px;font-size:14px;color:#374151;line-height:1.6'>
            <strong>Message:</strong><br/>
            {System.Net.WebUtility.HtmlEncode(message).Replace("\n", "<br/>")}
          </div>
          <p style='font-size:12px;color:#94a3b8;margin-top:16px'>Submitted: {DateTime.Now:dd MMM yyyy HH:mm} IST</p>
        </div>";

        // Use the injected email service's internal SMTP
        // We call it via reflection since EmailService.SendAsync is private.
        // For simplicity, duplicate the SMTP call here:
        var smtp = HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetSection("Smtp");
        using var client = new System.Net.Mail.SmtpClient(smtp["Host"], smtp.GetValue<int>("Port", 25))
        {
            DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new System.Net.NetworkCredential(smtp["FromEmail"], smtp["Password"])
        };

        var msg = new System.Net.Mail.MailMessage(
            new System.Net.Mail.MailAddress(smtp["FromEmail"]!, smtp["FromName"]),
            new System.Net.Mail.MailAddress(smtp["FromEmail"]!)) // send to admin inbox
        {
            Subject = $"[Contact Form] {queryType} — {name}",
            Body = body,
            IsBodyHtml = true,
            ReplyToList = { new System.Net.Mail.MailAddress(email, name) }
        };

        await client.SendMailAsync(msg);
    }

    private async Task SendUserAcknowledgementAsync(string name, string email, string queryType)
    {
        var smtp = HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetSection("Smtp");
        using var client = new System.Net.Mail.SmtpClient(smtp["Host"], smtp.GetValue<int>("Port", 25))
        {
            DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new System.Net.NetworkCredential(smtp["FromEmail"], smtp["Password"])
        };

        var body = $@"
        <div style='font-family:Poppins,sans-serif;max-width:480px;margin:40px auto;padding:32px;border:1px solid #e5e7eb;border-radius:12px'>
          <h2 style='color:#1e1b6e;margin-bottom:8px'>Thank you, {System.Net.WebUtility.HtmlEncode(name)}!</h2>
          <p style='color:#64748b;font-size:14px;line-height:1.6'>
            We've received your query about <strong>{System.Net.WebUtility.HtmlEncode(queryType)}</strong> and will get back to you within <strong>24 hours</strong>.
          </p>
          <p style='color:#64748b;font-size:14px;line-height:1.6'>
            For urgent matters, call us at <strong>+91 84455 88990</strong> (Mon–Sat, 9 AM – 6 PM IST).
          </p>
          <hr style='border:none;border-top:1px solid #e5e7eb;margin:20px 0'/>
          <p style='color:#94a3b8;font-size:12px'>
            S3KN Software Solutions Pvt. Ltd. · admin@gpmnrega.com<br/>
            Your data is handled in accordance with India's DPDP Act 2023. <a href='https://gpmnrega.com/privacypolicy' style='color:#4f46e5'>Privacy Policy</a>
          </p>
        </div>";

        var msg = new System.Net.Mail.MailMessage(
            new System.Net.Mail.MailAddress(smtp["FromEmail"]!, smtp["FromName"]),
            new System.Net.Mail.MailAddress(email, name))
        {
            Subject = "We received your message — GP MNREGA",
            Body = body,
            IsBodyHtml = true
        };

        await client.SendMailAsync(msg);
    }
}
