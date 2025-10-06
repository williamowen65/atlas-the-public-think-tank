using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.DataProtection;
using atlas_the_public_think_tank.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace atlas_the_public_think_tank.Areas.Identity.Pages.Account
{
    public class UnsubscribeFromEmailsModel : PageModel
    {
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ApplicationDbContext _context;

        public UnsubscribeFromEmailsModel(IDataProtectionProvider dataProtectionProvider, ApplicationDbContext context)
        {
            _dataProtectionProvider = dataProtectionProvider;
            _context = context;
        }

        public bool SubscriptionStatus { get; set; }
        public string Email { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                ErrorMessage = "Invalid unsubscribe link.";
                return Page();
            }

            try
            {
                var dataProtector = _dataProtectionProvider.CreateProtector("EmailUnsubscribeTokens");
                var decryptedToken = dataProtector.Unprotect(token);
                var parts = decryptedToken.Split('|');

                if (parts.Length != 3)
                {
                    ErrorMessage = "Invalid token format.";
                    return Page();
                }

                var userId = Guid.Parse(parts[0]);
                var emailId = Guid.Parse(parts[1]);
                var timestamp = long.Parse(parts[2]);

                // Optional: Check if token is expired (e.g., 30 days)
                var tokenDate = new DateTime(timestamp);
                if (DateTime.UtcNow - tokenDate > TimeSpan.FromDays(30))
                {
                    ErrorMessage = "This unsubscribe link has expired.";
                    return Page();
                }

                // Unsubscribe the user
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.SubscribedToEmail = false;
                    await _context.SaveChangesAsync();
                    SubscriptionStatus = false;
                    Email = user.Email!;
                }
                else
                {
                    ErrorMessage = "User not found.";
                }
            }
            catch (Exception)
            {
                ErrorMessage = "Invalid or corrupted unsubscribe token.";
            }

            return Page();
        }
    }
}