using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using ServiceGovernance.Registry.Tests.Helper;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class InMemoryXmlRepositoryDataProtectionBuilderExtensions
    {
        /// <summary>
        /// Configures the data protection system to persist keys to in memory list
        /// </summary>
        /// <param name="builder">The builder instance to modify.</param>        
        /// <returns>A reference to the <see cref="IDataProtectionBuilder" /> after this operation has completed.</returns>
        public static IDataProtectionBuilder PersistKeysToInMemory(this IDataProtectionBuilder builder)
        {
            builder.Services.Configure<KeyManagementOptions>(options =>
            {
                options.XmlRepository = new InMemoryXmlRepository();
            });

            return builder;
        }
    }
}
