using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.IO;
using System.Threading.Tasks;

namespace atlas_the_public_think_tank.Utilities
{
    public static class ViewRenderService
    {
        public static async Task<string> RenderViewToStringAsync<TModel>(
            IServiceProvider serviceProvider,
            HttpContext httpContext,
            string viewPath,
            TModel model)
        {
            var viewEngine = (ICompositeViewEngine)serviceProvider.GetService(typeof(ICompositeViewEngine));
            var tempDataProvider = (ITempDataProvider)serviceProvider.GetService(typeof(ITempDataProvider));
            var metadataProvider = (IModelMetadataProvider)serviceProvider.GetService(typeof(IModelMetadataProvider));

            var actionContext = new ActionContext(
                httpContext,
                httpContext.GetRouteData() ?? new Microsoft.AspNetCore.Routing.RouteData(),
                new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
            );

            var viewData = new ViewDataDictionary<TModel>(metadataProvider, new ModelStateDictionary())
            {
                Model = model
            };
            var tempData = new TempDataDictionary(httpContext, tempDataProvider);

            if (viewPath.StartsWith("~/"))
            {
                viewPath = viewPath.Substring(1);
            }

            var viewResult = viewEngine.GetView(executingFilePath: null, viewPath: viewPath, isMainPage: false);

            if (!viewResult.Success)
            {
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