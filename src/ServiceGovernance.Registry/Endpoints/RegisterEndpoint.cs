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
        public static PathString Path { get; } = new PathString("/v1/register");

        public RegisterEndpoint(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IServiceStore store, IRegistrationTokenProvider tokenProvider)
        {
            if (HttpMethods.IsPost(context.Request.Method))
            {
                await RegisterServiceAsync(context, store, tokenProvider);
            }
            else if (HttpMethods.IsDelete(context.Request.Method))
            {
                await UnregisterServiceAsync(context, store, tokenProvider);
            }
            else
            {
                // Call the next delegate/middleware in the pipeline
                await _next(context);
            }
        }

        private async Task UnregisterServiceAsync(HttpContext context, IServiceStore store, IRegistrationTokenProvider tokenProvider)
        {
            if (context.Request.Path.HasValue)
            {
                var registerToken = context.Request.Path.Value.Substring(1);

                var serviceRegistration = await tokenProvider.ValidateAsync(registerToken);

                if (serviceRegistration != null)
                {
                    var item = await store.FindByServiceIdAsync(serviceRegistration.ServiceIdentifier);

                    if (item != null)
                    {
                        // remove endpoints from service
                        item.ServiceEndpoints = item.ServiceEndpoints.Except(serviceRegistration.Endpoints).ToArray();
                        // remove ipaddress from service
                        item.IpAddresses = item.IpAddresses.Except(new[] { serviceRegistration.MachineIpAddress }).ToArray();

                        // remove service when no endpoints registered anymore
                        if (item.ServiceEndpoints.Length > 0)
                            await store.StoreAsync(item);
                        else
                            await store.RemoveAsync(serviceRegistration.ServiceIdentifier);
                    }
                }
            }
        }

        private async Task RegisterServiceAsync(HttpContext context, IServiceStore store, IRegistrationTokenProvider tokenProvider)
        {
            using (StreamReader sr = new StreamReader(context.Request.Body))
            {
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    JsonSerializer serializer = new JsonSerializer();

                    var model = serializer.Deserialize<ServiceRegistrationInputModel>(reader);

                    if (ValidateModel(model))
                    {
                        var service = await store.FindByServiceIdAsync(model.ServiceIdentifier);

                        if (service == null)
                        {
                            service = new Service()
                            {
                                DisplayName = model.ServiceDisplayName,
                                ServiceId = model.ServiceIdentifier,
                                ServiceEndpoints = model.Endpoints,
                                IpAddresses = new[] { model.MachineIpAddress }
                            };
                        }
                        else
                        {
                            service.ServiceEndpoints = service.ServiceEndpoints.Concat(model.Endpoints).ToArray();
                            service.IpAddresses = service.IpAddresses.Concat(new[] { model.MachineIpAddress }).ToArray();
                        }

                        await store.StoreAsync(service);

                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync(await tokenProvider.GenerateAsync(model));
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
