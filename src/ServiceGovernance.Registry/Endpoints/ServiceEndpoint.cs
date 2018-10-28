using Microsoft.AspNetCore.Http;
using ServiceGovernance.Registry.Services;
using System.Threading.Tasks;

namespace ServiceGovernance.Registry.Endpoints
{
    /// <summary>
    /// Middleware providing the "service" API endpoint
    /// </summary>
    public class ServiceEndpoint : IMiddleware
    {
        private readonly IServiceRegistry _serviceRegistry;

        /// <summary>
        /// Gets the url path this endpoint is listening on
        /// </summary>
        public static PathString Path { get; } = new PathString("/v1/service");

        public ServiceEndpoint(IServiceRegistry serviceRegistry)
        {
            _serviceRegistry = serviceRegistry ?? throw new System.ArgumentNullException(nameof(serviceRegistry));
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (HttpMethods.IsGet(context.Request.Method))
            {
                if (!context.Request.Path.HasValue)
                    await GetAllServicesAsync(context);
                else
                    await GetServiceAsync(context, context.Request.Path.Value.Substring(1));
            }
            else
            {
                await next.Invoke(context);
            }
        }

        private async Task GetServiceAsync(HttpContext context, string serviceId)
        {
            var service = await _serviceRegistry.GetServiceAsync(serviceId);

            if (service != null)
            {
                await context.WriteModelAsync(service);
            }
            else
            {
                context.Response.StatusCode = 404;
            }
        }

        private async Task GetAllServicesAsync(HttpContext context)
        {
            await context.WriteModelAsync(await _serviceRegistry.GetAllServicesAsync());
        }
    }
}
