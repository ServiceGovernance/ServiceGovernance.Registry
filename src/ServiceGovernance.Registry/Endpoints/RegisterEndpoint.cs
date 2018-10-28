using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using ServiceGovernance.Registry.Models;
using ServiceGovernance.Registry.Services;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ServiceGovernance.Registry.Endpoints
{
    /// <summary>
    /// Middleware providing the "register" API endpoint
    /// </summary>
    public class RegisterEndpoint : IMiddleware
    {
        private readonly IServiceRegistry _serviceRegistry;

        /// <summary>
        /// Gets the url path this endpoint is listening on
        /// </summary>
        public static PathString Path { get; } = new PathString("/v1/register");

        public RegisterEndpoint(IServiceRegistry serviceRegistry)
        {
            _serviceRegistry = serviceRegistry ?? throw new System.ArgumentNullException(nameof(serviceRegistry));
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (HttpMethods.IsPost(context.Request.Method))
            {
                await RegisterServiceAsync(context);
            }
            else if (HttpMethods.IsDelete(context.Request.Method))
            {
                await UnregisterServiceAsync(context);
            }
            else
            {
                // Call the next delegate/middleware in the pipeline
                await next(context);
            }
        }

        private async Task UnregisterServiceAsync(HttpContext context)
        {
            if (context.Request.Path.HasValue)
            {
                var registerToken = context.Request.Path.Value.Substring(1);

                await _serviceRegistry.Unregister(registerToken);
            }
        }

        private async Task RegisterServiceAsync(HttpContext context)
        {
            using (StreamReader sr = new StreamReader(context.Request.Body))
            {
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    JsonSerializer serializer = new JsonSerializer();

                    var model = serializer.Deserialize<ServiceRegistrationInputModel>(reader);

                    if (ValidateModel(model))
                    {
                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync(await _serviceRegistry.RegisterAsync(model));
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync("Invalid registration model");
                    }
                }
            }
        }

        private bool ValidateModel(ServiceRegistrationInputModel model)
        {
            if (string.IsNullOrWhiteSpace(model.ServiceId))
                return false;

            if (model.Endpoints == null || model.Endpoints.Length == 0)
                return false;

            return true;
        }
    }
}
