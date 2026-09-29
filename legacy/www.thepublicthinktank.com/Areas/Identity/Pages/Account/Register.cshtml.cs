// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Email;
using atlas_the_public_think_tank.Email.Models;
using atlas_the_public_think_tank.Models.ViewModel.UI_VM;
using atlas_the_public_think_tank.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using static atlas_the_public_think_tank.Email.EmailLogger;

namespace atlas_the_public_think_tank.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserStore<AppUser> _userStore;
        private readonly IUserEmailStore<AppUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly EmailLogger _emailLogger;
        private readonly IOptions<EmailSettings> _emailSettings;
        private readonly IWebHostEnvironment _env;

        public RegisterModel(
            UserManager<AppUser> userManager,
            IUserStore<AppUser> userStore,
            SignInManager<AppUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            EmailLogger emailQueue,
            IOptions<EmailSettings> EmailSettings,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailLogger = emailQueue;
            _emailSettings = EmailSettings;
            _env = env;
        }

        public Alert_VM? RegistrationEnvironmentAlert
        {
            get
            {
                // Get access to the environment name 
                if (_env.EnvironmentName == "Testing")
                {
                    return new Alert_VM()
                    { 
                        Message = "<div class='flex-column d-flex justify-content-center align-items-center'>By signing up, you are becoming a tester of this app.  <a href='/how-to-be-a-tester' class='btn btn-primary mt-2'>Learn about being a tester</a></div>",
                        Type = Models.Enums.AlertType.warning,
                        IsFullWidth = true
                    };
                }
                if (_env.EnvironmentName == "Development")
                {
                    return new Alert_VM()
                    {
                        Message = "<div class='flex-column d-flex justify-content-center align-items-center'>This is a development version of the app on your local machine<a href='/how-to-be-a-tester' class='btn btn-primary mt-2'>Learn about being a tester</a></div>",
                        Type = Models.Enums.AlertType.warning,
                        IsFullWidth = true
                    };
                }
                if (_env.EnvironmentName == "Staging")
                {
                    return new Alert_VM()
                    { 
                        Message = "This is a staging version of the app.",
                        Type = Models.Enums.AlertType.warning,
                        IsFullWidth = true
                    };
                }


                return null;
            }
            set { } // Empty setter to satisfy property syntax
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {

            [Required]
            [Display(Name = "Username")]
            [StringLength(50, MinimumLength = 3,
            ErrorMessage = "Display name must be between 3 and 50 characters.")]
            [RegularExpression(@"^[A-Za-z0-9 _\-]+$",
            ErrorMessage = "Only letters, numbers, spaces, underscores and dashes are allowed.")]
            public string UserName { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                // Check for existing username
                var existingUserByName = await _userManager.FindByNameAsync(Input.UserName);
                if (existingUserByName != null)
                {
                    ModelState.AddModelError("Input.UserName", "Username is already taken.");
                }
                // Check for existing email
                var existingUserByEmail = await _userManager.FindByEmailAsync(Input.Email);
                if (existingUserByEmail != null)
                {
                    ModelState.AddModelError("Input.Email", "Email is already registered.");
                }
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                var user = CreateUser();
                await _userStore.SetUserNameAsync(user, Input.UserName, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme,
                        host: _emailSettings.Value.BaseUrl);

                    ConfirmationEmailModel confirmationEmailModel = new ConfirmationEmailModel()
                    {
                        ConfirmationLink = callbackUrl,
                        UserName = Input.UserName
                    };

                    EmailInfo emailInfo = new Emails.ConfirmationEmail(user, confirmationEmailModel);

                    await _emailLogger.SendEmailToUser(Input.Email, emailInfo);


                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private AppUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<AppUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(AppUser)}'. " +
                    $"Ensure that '{nameof(AppUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<AppUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<AppUser>)_userStore;
        }
    }
}
