using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;

namespace qckdev.QuerySpecs
{
    /// <summary>
    /// Registration extensions for query specs processors and query specs engine.
    /// </summary>
    public static class QuerySpecsServiceCollectionExtensions
    {
        /// <summary>
        /// Registers query specs services using the provided configuration.
        /// </summary>
        /// <param name="services">Service collection to configure.</param>
        /// <param name="configure">Configuration callback for mapping and assembly scanning.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddQuerySpecs(
            this IServiceCollection services,
            Action<QuerySpecsConfiguration> configure)
        {
            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var specsConfiguration = new QuerySpecsConfiguration();
            configure(specsConfiguration);

            Microsoft.Extensions.DependencyInjection.OptionsServiceCollectionExtensions
                .Configure<QuerySpecsEngineOptions>(services, options =>
                {
                    options.Map = specsConfiguration.Map;
                });

            foreach (var assembly in specsConfiguration.AssembliesToRegister.Distinct())
            {
                var implementations = assembly.DefinedTypes
                    .Where(type => type.IsClass && !type.IsAbstract)
                    .Select(type => new
                    {
                        Implementation = type.AsType(),
                        Contracts = type.ImplementedInterfaces
                            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuerySpecsProcessor<,>))
                            .ToArray()
                    })
                    .Where(x => x.Contracts.Length != 0);

                foreach (var implementation in implementations)
                {
                    foreach (var contract in implementation.Contracts)
                    {
                        services.AddScoped(contract, implementation.Implementation);
                    }
                }
            }

            services.TryAddScoped<IQuerySpecsEngine, QuerySpecsEngine>();
            return services;
        }
    }
}
