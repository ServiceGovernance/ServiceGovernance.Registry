using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ServiceGovernance.Registry.Endpoints
{
    /// <summary>
    /// Middleware providing the "service" API endpoint
    /// </summary>
    public class ServiceEndpoint
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Gets the url path this endpoint is listening on
        /// </summary>
        public static PathString Path { get; } = new PathString("/service");

        public ServiceEndpoint(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }
    }
}
