using System.Net.Mail;

namespace atlas_the_public_think_tank.Email.Infrastructure
{

    /// <summary>
    /// This an interface to compliment the IEmailSender
    /// </summary>
    public interface IAppEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
        Task SendEmailWithAttachmentsAsync(string email, string subject, string htmlMessage, IEnumerable<Attachment> attachments);
    }
}
