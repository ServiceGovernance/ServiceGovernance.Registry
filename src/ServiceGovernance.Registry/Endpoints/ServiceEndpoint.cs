﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceGovernance.Registry.Stores;
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

        public async Task InvokeAsync(HttpContext context, IServiceStore store)
        {
            if (HttpMethods.IsGet(context.Request.Method))
            {
                if (!context.Request.Path.HasValue)
                    await GetAllServicesAsync(context, store);
                else
                    await GetServiceAsync(context, store, context.Request.Path.Value.Substring(1));
            }
            else
                await _next.Invoke(context);
        }

        private async Task GetServiceAsync(HttpContext context, IServiceStore store, string serviceId)
        {
            var service = await store.FindByServiceIdAsync(serviceId);

            if (service != null)
                await context.WriteModelAsync(service);
            else
                await context.ExecuteResultAsync(new NotFoundResult());
        }

        private async Task GetAllServicesAsync(HttpContext context, IServiceStore store)
        {
            var services = await store.GetAllAsync();

            await context.WriteModelAsync(services);
        }
    }
}
