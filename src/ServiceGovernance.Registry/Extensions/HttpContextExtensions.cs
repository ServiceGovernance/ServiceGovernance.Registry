using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace ServiceGovernance.Registry
{
    /// <summary>
    /// Helper methods for HttpContext
    /// </summary>
    public static class HttpContextExtensions
    {
        private static readonly RouteData EmptyRouteData = new RouteData();
        private static readonly ActionDescriptor EmptyActionDescriptor = new ActionDescriptor();

        /// <summary>
        /// Writes the model to the response
        /// </summary>
        /// <typeparam name="TModel">The type of the model to write.</typeparam>
        /// <param name="context">The http context.</param>
        /// <param name="model">The model which should be written to the response.</param>
        /// <returns></returns>
        public static Task WriteModelAsync<TModel>(this HttpContext context, TModel model)
        {
            var result = new ObjectResult(model)
            {
                DeclaredType = typeof(TModel)
            };

            return context.ExecuteResultAsync(result);
        }

        /// <summary>
        /// Executes the result object (like FileResult or OkResult)
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="context">The http context.</param>
        /// <param name="result">The action result instance.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// context
        /// or
        /// result
        /// </exception>
        public static Task ExecuteResultAsync<TResult>(this HttpContext context, TResult result) where TResult : class, IActionResult
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            var executor = context.RequestServices.GetRequiredService<IActionResultExecutor<TResult>>();

            var routeData = context.GetRouteData() ?? EmptyRouteData;
            var actionContext = new ActionContext(context, routeData, EmptyActionDescriptor);

            return executor.ExecuteAsync(actionContext, result);
        }
    }
}
