using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceGovernance.Registry.Configuration;
using ServiceGovernance.Registry.Endpoints;
using ServiceGovernance.Registry.Services;
using ServiceGovernance.Registry.Stores;
using System;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Pipeline extension methods for adding ServiceRegistry
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds ServiceRegistry to the pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseServiceRegistry(this IApplicationBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            app.Validate();

            app.Map(RegisterEndpoint.Path, b => b.UseMiddleware<RegisterEndpoint>());
            app.Map(ServiceEndpoint.Path, b => b.UseMiddleware<ServiceEndpoint>());

            app.SelfRegister();

            return app;
        }

        internal static void Validate(this IApplicationBuilder app)
        {
            if (!(app.ApplicationServices.GetService(typeof(ILoggerFactory)) is ILoggerFactory loggerFactory))
                throw new InvalidOperationException(nameof(loggerFactory));

            var logger = loggerFactory.CreateLogger("ServiceRegistry.Startup");

            var scopeFactory = app.ApplicationServices.GetService<IServiceScopeFactory>();

            using (var scope = scopeFactory.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                TestService(serviceProvider, typeof(IServiceStore), logger, "No storage mechanism for services specified. Use the 'AddInMemoryServices' extension method to register a development version or provide an implementation for 'IServiceStore'.");
            }
        }

        /// <summary>
        /// Registers the service registry service itself in the store
        /// </summary>
        /// <param name="app"></param>
        internal static void SelfRegister(this IApplicationBuilder app)
        {
            var scopeFactory = app.ApplicationServices.GetService<IServiceScopeFactory>();

            using (var scope = scopeFactory.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                var options = serviceProvider.GetRequiredService<ServiceRegistryOptions>();

                if (options.SelfRegistration.SelfRegister)
                {
                    var registry = serviceProvider.GetRequiredService<IServiceRegistry>();
                    var registerToken = registry.RegisterAsync(new ServiceGovernance.Registry.Models.ServiceRegistrationInputModel
                    {
                        ServiceId = options.SelfRegistration.ServiceId,
                        DisplayName = options.SelfRegistration.DisplayName,
                        Endpoints = options.SelfRegistration.Endpoints ?? SelfRegisterOptions.GetServiceEndpoints(serviceProvider.GetRequiredService<IServer>()),
                        PublicUrls = options.SelfRegistration.PublicUrls,
                        IpAddress = SelfRegisterOptions.GetIpAddress()
                    }).GetAwaiter().GetResult();

                    var lifetime = serviceProvider.GetRequiredService<IApplicationLifetime>();
                    lifetime.ApplicationStopping.Register(() => registry.UnregisterAsync(registerToken).GetAwaiter().GetResult());
                }
            }
        }

        internal static object TestService(IServiceProvider serviceProvider, Type service, ILogger logger, string message)
        {
            var appService = serviceProvider.GetService(service);

            if (appService == null)
            {
                logger.LogCritical(message);

                throw new InvalidOperationException(message);
            }

            return appService;
        }
    }
}
