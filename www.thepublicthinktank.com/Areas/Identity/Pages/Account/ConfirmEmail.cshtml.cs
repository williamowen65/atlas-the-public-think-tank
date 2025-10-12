// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Email;
using atlas_the_public_think_tank.Email.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static atlas_the_public_think_tank.Email.EmailLogger;

namespace atlas_the_public_think_tank.Areas.Identity.Pages.Account
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager; 
        private readonly IEmailSender _emailSender;
        private readonly EmailLogger _emailQueue;
        public ConfirmEmailModel(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailSender emailSender, EmailLogger emailQueue) 
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _emailQueue = emailQueue;
        }

        [TempData]
        public string StatusMessage { get; set; }
        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);

            // Log user in
            await _signInManager.SignInAsync(user, isPersistent: false); // Use SignInManager instead

            // Update User EmailSubscriptionStatus
            user.SubscribedToEmail = true;
            await _userManager.UpdateAsync(user);

            WelcomeEmailModel welcomeEmailModel = new WelcomeEmailModel()
            {
                UserName = user.UserName
            };

            EmailInfo emailInfo = new Emails.WelcomeEmail(user, welcomeEmailModel);

            await _emailQueue.SendEmailToUser(user.Email, emailInfo);


            StatusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";
            return Page();
        }
    }
}
