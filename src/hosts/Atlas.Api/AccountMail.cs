using System.Net;
using System.Net.Mail;

namespace Atlas.Api;

/// <summary>Account email delivery. SMTP is explicit; development files remain local-only.</summary>
public sealed class AccountMail(IConfiguration configuration, DevelopmentAccountMail development)
{
    private string? Setting(string name) => configuration[$"Atlas:AccountMail:{name}"];
    private bool UsesSmtp => string.Equals(Setting("Transport"), "Smtp", StringComparison.OrdinalIgnoreCase);
    public bool IsAvailable => UsesSmtp
        ? !string.IsNullOrWhiteSpace(Setting("Host"))
          && !string.IsNullOrWhiteSpace(Setting("Username"))
          && !string.IsNullOrWhiteSpace(Setting("Password"))
          && MailAddress.TryCreate(Setting("From"), out _)
          && Uri.TryCreate(Setting("PublicOrigin"), UriKind.Absolute, out var origin)
          && origin.Scheme == "https" && origin.AbsolutePath == "/"
          && string.IsNullOrEmpty(origin.Query) && string.IsNullOrEmpty(origin.Fragment)
          && string.IsNullOrEmpty(origin.UserInfo)
          && int.TryParse(Setting("Port") ?? "587", out var port) && port is > 0 and <= 65535
        : string.IsNullOrWhiteSpace(Setting("Transport")) && development.IsAvailable;

    public async Task SendAsync(string purpose, string email, Guid userId, string token, CancellationToken cancellationToken)
    {
        if (!IsAvailable) throw new InvalidOperationException("Account email delivery is not configured.");
        if (!UsesSmtp)
        {
            await development.WriteAsync(purpose, userId, token, cancellationToken);
            return;
        }
        var confirm = purpose == "confirm-email";
        // Use an operator-configured origin, never the request Host or a caller's return URL.
        // Fragments keep credentials out of web-server request paths and referrer headers.
        var link = Setting("PublicOrigin")!.TrimEnd('/')
            + (confirm ? "/confirm-email#" : "/reset-password#")
            + (confirm ? "userId=" + userId : "email=" + Uri.EscapeDataString(email))
            + "&token=" + Uri.EscapeDataString(token);
        using var message = new MailMessage();
        message.From = new MailAddress(Setting("From")!, "Atlas");
        message.To.Add(new MailAddress(email));
        message.Subject = confirm ? "Confirm your Atlas email" : "Reset your Atlas password";
        message.Body = (confirm ? "Confirm your email address" : "Choose a new password")
            + " using this link:\n\n" + link
            + "\n\nIf you did not request this, you can ignore this email.";
        using var smtp = new SmtpClient(Setting("Host")!, int.Parse(Setting("Port") ?? "587"))
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(Setting("Username"), Setting("Password")),
            UseDefaultCredentials = false
        };
        try { await smtp.SendMailAsync(message, cancellationToken); }
        catch (SmtpException)
        {
            // SMTP exceptions can contain recipient addresses or provider responses.
            throw new InvalidOperationException("Account email delivery failed. Please retry using resend confirmation or password recovery.");
        }
    }
}
