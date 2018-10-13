using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using ServiceGovernance.Registry.Models;
using ServiceGovernance.Registry.Stores;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ServiceGovernance.Registry.Endpoints
{
    /// <summary>
    /// Middleware providing the "register" API endpoint
    /// </summary>
    public class RegisterEndpoint
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Gets the url path this endpoint is listening on
        /// </summary>
        public static PathString Path { get; } = new PathString("/register");

        public RegisterEndpoint(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IServiceStore store)
        {
            if (HttpMethods.IsPost(context.Request.Method))
                await RegisterServiceAsync(context, store);
            else if (HttpMethods.IsDelete(context.Request.Method))
                await UnregisterServiceAsync(context, store);

            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }

        private Task UnregisterServiceAsync(HttpContext context, IServiceStore store)
        {
            var registerToken = context.Request.Path.Value;

            // validate token
            // retrieve serviceId, urls from token

            return Task.CompletedTask;
        }

        private async Task RegisterServiceAsync(HttpContext context, IServiceStore store)
        {
            using (StreamReader sr = new StreamReader(context.Request.Body))
            {
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    JsonSerializer serializer = new JsonSerializer();

                    var model = serializer.Deserialize<ServiceRegistrationInputModel>(reader);

                    await store.StoreAsync(new Service {
                        DisplayName = model.ServiceDisplayName,
                        ServiceId = model.ServiceIdentifier,
                        ServiceEndpoints = model.ServiceUrls,
                    });
                }
            }

            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("TODO RegisterToken");
        }
    }
}
