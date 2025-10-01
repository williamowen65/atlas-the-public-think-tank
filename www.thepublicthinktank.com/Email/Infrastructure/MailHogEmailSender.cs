using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;

namespace atlas_the_public_think_tank.Email.Infrastructure
{
    public class MailHogEmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using var client = new SmtpClient("localhost", 1025)
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = false
            };

            var mail = new MailMessage("noreply@thepublicthinktank.com", to, subject, body)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(mail);
        }
    }
}
