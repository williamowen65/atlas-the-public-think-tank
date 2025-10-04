using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace atlas_the_public_think_tank.Email.Infrastructure
{
    public class SMTPEmailSender : IEmailSender
    {

        private readonly IConfiguration _configuration;
        private readonly string _smtp_host;
        private readonly string _smtp_username;
        private readonly string _smtp_password;

        public SMTPEmailSender(IConfiguration configuration)
        {
            _configuration = configuration;

            string? smtp_host = _configuration["SMTP_HOST"];
            string? smtp_username = _configuration["SMTP_USERNAME"];
            string? smtp_password = _configuration["SMTP_PASSWORD"];

            if (smtp_host == null) {
                throw new Exception("SMTP_HOST not configured in environment variables");
            }

            if (smtp_username == null) {
                throw new Exception("SMTP_USERNAME not configured in environment variables");
            }

            if (smtp_password == null) {
                throw new Exception("SMTP_PASSWORD not configured in environment variables");
            }

            _smtp_host = smtp_host;
            _smtp_username = smtp_username;
            _smtp_password = smtp_password;

        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtp = new SmtpClient(_smtp_host, 587)
            {
                Credentials = new NetworkCredential(_smtp_username, _smtp_password),
                EnableSsl = true
            };

            var message = new MailMessage
            {
                From = new MailAddress("atlas@thepublicthinktank.com", "Atlas: The Public Think Tank"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            message.To.Add(email);

            await smtp.SendMailAsync(message);
        }
    }
}
