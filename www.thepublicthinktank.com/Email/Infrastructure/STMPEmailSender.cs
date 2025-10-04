using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace atlas_the_public_think_tank.Email.Infrastructure
{
    public class STMPEmailSender : IEmailSender
    {

        private readonly IConfiguration _configuration;
        private readonly string _stmp_password;

        public STMPEmailSender(IConfiguration configuration)
        {
            _configuration = configuration;

            string? stmp_password = _configuration["STMP_PASSWORD"];

            if (stmp_password == null) {
                throw new Exception("STMP_PASSWORD not configured in environment variables");
            }

            _stmp_password = stmp_password;

        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtp = new SmtpClient("mail.privateemail.com", 465)
            {
                Port = 587,
                Credentials = new NetworkCredential("atlas@thepublicthinktank.com", _stmp_password),
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
