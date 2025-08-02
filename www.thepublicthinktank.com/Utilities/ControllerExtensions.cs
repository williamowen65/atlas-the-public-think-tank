using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace atlas_the_public_think_tank.Utilities
{
    public static class ControllerExtensions
    {

        /// <summary>
        /// This provides the option to render a view while still within controller routes.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="controller"></param>
        /// <param name="viewPath"></param>
        /// <param name="model"></param>
        /// <returns>String</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<string> RenderViewToStringAsync<TModel>(this Controller controller, string viewPath, TModel model)
        {
            if (string.IsNullOrEmpty(viewPath))
            {
                viewPath = controller.ControllerContext.ActionDescriptor.ActionName;
            }

            controller.ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                IViewEngine viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;

                // GetView is designed to work with absolute paths (starting with '/')
                // If the path starts with ~/, convert it to an absolute path
                if (viewPath.StartsWith("~/"))
                {
                    viewPath = viewPath.Substring(1); // Remove the ~ character
                }

                ViewEngineResult viewResult = viewEngine.GetView(executingFilePath: null, viewPath: viewPath, isMainPage: false);

                // If the view wasn't found, try FindView as a fallback
                if (viewResult.View == null)
                {
                    // Try removing extension if it exists
                    if (viewPath.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
                    {
                        viewPath = viewPath.Substring(0, viewPath.Length - 7);
                    }

                    viewResult = viewEngine.FindView(controller.ControllerContext, viewPath, false);
                }

                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"View not found. Path: {viewPath}");
                }

                ViewContext viewContext = new ViewContext(
                    controller.ControllerContext,
                    viewResult.View,
                    controller.ViewData,
                    controller.TempData,
                    sw,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return sw.ToString();
            }
        }
    }
}
