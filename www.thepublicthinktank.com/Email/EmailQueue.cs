using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.DbContext;
using atlas_the_public_think_tank.Email.Models;
using atlas_the_public_think_tank.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.Serialization;

namespace atlas_the_public_think_tank.Email
{
    public class EmailQueue
    {
        public readonly IEmailSender _emailSender;
        public readonly IServiceProvider _serviceProvider;
        public readonly IHttpContextAccessor _httpContextAccessor;
        public readonly ApplicationDbContext _context;
        public EmailQueue(IEmailSender emailSender, IServiceProvider serviceProvider, ApplicationDbContext context, IHttpContextAccessor httpContextAccessor = null)
        {

            // Confirm IEmailService is not NoOpEmailSender
            if (emailSender.GetType().Name == "NoOpEmailSender")
            {
                throw new InvalidOperationException("IEmailSender is set to NoOpEmailSender. Please configure a real email sender implementation.");
            }

            _emailSender = emailSender;
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }



        /// <summary>
        /// All emails for the app are sent via this method.
        /// It wraps the IEmailService logic and is used to keep track of emails sent
        /// and prevent accidental duplicate emails
        /// </summary>
        public async Task Send(string emailAddress, EmailInfo emailInfo) {

            // Check if the email has already been sent
            var alreadySent = await _context.EmailLog
                .AnyAsync(log => 
                    log.UserID == emailInfo.User.Id &&
                    log.EmailID == emailInfo.EmailID && 
                    log.EmailStatus == EmailStatus.Sent);

            if (alreadySent)
            {
                // Optionally, you can throw, return, or just exit
                return;
            }

            HttpContext? httpContext = _httpContextAccessor.HttpContext;
            string emailRendered = await ViewRenderService.RenderViewToStringAsync(_serviceProvider, httpContext, emailInfo.TemplatePath, emailInfo.Model);
            await _emailSender.SendEmailAsync(emailAddress, emailInfo.Subject, emailRendered);

            // Log the email sent in the email log
            _context.EmailLog.Add(new EmailLog() 
            { 
                EmailID = emailInfo.EmailID,
                UserID = emailInfo.User.Id,
                SentAt = DateTime.UtcNow,
                EmailStatus = EmailStatus.Sent
            });

            await _context.SaveChangesAsync();
        }

        public static class Emails
        {


            public class WelcomeEmail : EmailInfo
            {
                public WelcomeEmail(AppUser user, dynamic model)
                {
                    Subject = "Welcome to Atlas: The Public Think Tank";
                    TemplatePath = @"Email/Templates/WelcomeEmail.cshtml";
                    EmailID = new Guid("E1472871-882F-46C5-9B4E-2A37824BB025");
                    User = user;
                    Model = model;
                }
            }

            public class ConfirmationEmail : EmailInfo
            {   
                public ConfirmationEmail(AppUser user, dynamic model) 
                {
                    Subject = "Confirm your email";
                    TemplatePath = @"Email/Templates/ConfirmationEmail.cshtml";
                    EmailID = new Guid("C510CE53-A64C-4B2D-8FF9-4F8BAC8ADF19");
                    User = user;
                    Model = model;
                }
                
            }
        }


        public class EmailInfo
        { 
            public string Subject { get; set; }
            public string TemplatePath { get; set; }

            public Guid EmailID { get; set; }

            public AppUser User { get; set; }
            public dynamic Model { get; set; }
        }

    }
}
