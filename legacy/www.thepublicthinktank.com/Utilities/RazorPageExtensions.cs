using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
// Remove this line if it causes an error or is not found in your project:
// using Microsoft.AspNetCore.Mvc.ViewFeatures.Infrastructure;

namespace atlas_the_public_think_tank.Utilities
{
    public static class RazorPageExtensions
    {
        public static async Task<string> RenderViewToStringAsync<TModel>(
            this PageModel pageModel,
            string viewPath,
            TModel model)
        {
            var serviceProvider = pageModel.HttpContext.RequestServices;
            var viewEngine = (ICompositeViewEngine)serviceProvider.GetService(typeof(ICompositeViewEngine));
            var tempDataProvider = (ITempDataProvider)serviceProvider.GetService(typeof(ITempDataProvider));
            var metadataProvider = (IModelMetadataProvider)serviceProvider.GetService(typeof(IModelMetadataProvider));

            var actionContext = new ActionContext(
                pageModel.HttpContext,
                pageModel.RouteData,
                pageModel.PageContext.ActionDescriptor
            );

            // Create ViewDataDictionary directly
            var viewData = new ViewDataDictionary<TModel>(metadataProvider, pageModel.ModelState)
            {
                Model = model
            };
            var tempData = new TempDataDictionary(pageModel.HttpContext, tempDataProvider);

            if (viewPath.StartsWith("~/"))
            {
                viewPath = viewPath.Substring(1);
            }

            var viewResult = viewEngine.GetView(executingFilePath: null, viewPath: viewPath, isMainPage: false);

            if (!viewResult.Success)
            {
                // Try removing extension if it exists
                if (viewPath.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
                {
                    viewPath = viewPath.Substring(0, viewPath.Length - 7);
                }
                viewResult = viewEngine.FindView(actionContext, viewPath, false);
            }

            if (!viewResult.Success)
            {
                throw new ArgumentNullException($"View not found. Path: {viewPath}");
            }

            using (var sw = new StringWriter())
            {
                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    viewData,
                    tempData,
                    sw,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return sw.ToString();
            }
        }
    }
}