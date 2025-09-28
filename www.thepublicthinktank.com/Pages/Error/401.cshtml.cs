using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace atlas_the_public_think_tank.Pages.Error
{
    public class UnauthorizedModel : PageModel
    {

        public string? OriginalPath { get; private set; }
        public void OnGet()
        {
            var feature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            var query = string.IsNullOrEmpty(feature?.OriginalQueryString) ? "" : feature!.OriginalQueryString;
            OriginalPath = $"{feature?.OriginalPath}{query}";
        }
    }
}
