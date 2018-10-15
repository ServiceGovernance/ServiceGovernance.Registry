using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using ServiceGovernance.Registry.Models;
using ServiceGovernance.Registry.Services;
using ServiceGovernance.Registry.Stores;
using System.IO;
using System.Linq;
using System.Net;
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

        public async Task InvokeAsync(HttpContext context, IServiceStore store, IRegistrationTokenProvider tokenProvider)
        {
            if (HttpMethods.IsPost(context.Request.Method))
                await RegisterServiceAsync(context, store, tokenProvider);
            else if (HttpMethods.IsDelete(context.Request.Method))
                await UnregisterServiceAsync(context, store, tokenProvider);
            else
                // Call the next delegate/middleware in the pipeline
                await _next(context);
        }

        private async Task UnregisterServiceAsync(HttpContext context, IServiceStore store, IRegistrationTokenProvider tokenProvider)
        {
            var registerToken = context.Request.Path.Value;

            var service = await tokenProvider.ValidateAsync(registerToken);

            if (service != null)
            {
                var item = await store.FindByServiceIdAsync(service.ServiceId);

                if (item != null)
                {
                    // remove endpoints from service
                    item.ServiceEndpoints = item.ServiceEndpoints.Except(service.ServiceEndpoints).ToArray();

                    // remove service when no endpoints registered anymore
                    if (item.ServiceEndpoints.Length > 0)
                        await store.StoreAsync(item);
                    else
                        await store.RemoveAsync(service.ServiceId);
                }
            }
        }

        private async Task RegisterServiceAsync(HttpContext context, IServiceStore store, IRegistrationTokenProvider tokenProvider)
        {
            Service service = null;

            using (StreamReader sr = new StreamReader(context.Request.Body))
            {
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    JsonSerializer serializer = new JsonSerializer();

                    var model = serializer.Deserialize<ServiceRegistrationInputModel>(reader);

                    if (ValidateModel(model))
                    {
                        service = new Service()
                        {
                            DisplayName = model.ServiceDisplayName,
                            ServiceId = model.ServiceIdentifier,
                            ServiceEndpoints = model.Endpoints,
                        };

                        await store.StoreAsync(service);

                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync(await tokenProvider.GenerateAsync(service));
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
            if (string.IsNullOrWhiteSpace(model.ServiceIdentifier))
                return false;

            if (model.Endpoints == null || model.Endpoints.Length == 0)
                return false;

            return true;
        }
    }
}
