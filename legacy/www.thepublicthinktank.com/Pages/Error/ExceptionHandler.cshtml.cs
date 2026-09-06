using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace atlas_the_public_think_tank.Pages.Error
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ExceptionHandlerModel : PageModel
    {
        public string ExceptionMessage { get; set; } = "An error occurred";
        public string ExceptionType { get; set; } = "Error";
        public string? InnerExceptionMessage { get; set; }
        public string RequestId { get; set; }

        public void OnGet()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionFeature?.Error != null)
            {
                ExceptionType = exceptionFeature.Error.GetType().Name;
                ExceptionMessage = exceptionFeature.Error.Message;

                // Get inner exception if it exists
                if (exceptionFeature.Error.InnerException != null)
                {
                    InnerExceptionMessage = exceptionFeature.Error.InnerException.Message;
                }
            }

            RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }
}