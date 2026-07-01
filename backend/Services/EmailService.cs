using MailKit.Net.Smtp;
using MimeKit;

namespace ClimateAdvisor.Api.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendContactNotificationAsync(string name, string email, string subject, string message, CancellationToken ct = default)
    {
        await SendRawAsync(
            _config["Email:ToAddress"] ?? "dkcngomes@gmail.com",
            $"[Climate Survival] {subject}",
            $"""
New contact form submission:

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
From:    {name} ({email})
Subject: {subject}
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

{message}

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Sent via Climate Survival Contact Form
""",
            ct
        );
    }

    public async Task SendRawAsync(string toAddress, string subject, string body, CancellationToken ct = default)
    {
        var smtpHost = _config["Email:SmtpHost"];
        var smtpPortStr = _config["Email:SmtpPort"];
        var smtpUser = _config["Email:SmtpUser"];
        var smtpPass = _config["Email:SmtpPass"];

        if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
        {
            _logger.LogWarning("SMTP not configured — cannot send email to {To}", toAddress);
            return;
        }

        var smtpPort = int.TryParse(smtpPortStr, out var p) ? p : 587;

        try
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress("Climate Survival Alerts", smtpUser));
            mimeMessage.To.Add(MailboxAddress.Parse(toAddress));
            mimeMessage.Subject = subject;
            mimeMessage.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(smtpUser, smtpPass, ct);
            await client.SendAsync(mimeMessage, ct);
            await client.DisconnectAsync(true, ct);

            _logger.LogInformation("Email sent to {To}, subject: {Subject}", toAddress, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}, subject: {Subject}", toAddress, subject);
        }
    }
}
