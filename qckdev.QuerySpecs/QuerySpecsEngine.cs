using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Reflection;

namespace qckdev.QuerySpecs
{
    internal sealed class QuerySpecsEngine : IQuerySpecsEngine
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly QuerySpecsEngineOptions _options;

        public QuerySpecsEngine(IServiceProvider serviceProvider, IOptions<QuerySpecsEngineOptions> options)
        {
            _serviceProvider = serviceProvider;
            _options = options.Value;
        }

        public TSpecs Parse<TSource, TSpecs>(TSource source)
        {
            if (source is TSpecs alreadyParsed)
            {
                return alreadyParsed;
            }

            if (_options.Map is null)
            {
                throw new InvalidOperationException(
                    $"No specs mapper is configured to parse '{typeof(TSource).Name}' into '{typeof(TSpecs).Name}'.");
            }

            var mapped = _options.Map(_serviceProvider, source!, typeof(TSpecs));
            if (mapped is TSpecs parsed)
            {
                return parsed;
            }

            throw new InvalidOperationException(
                $"Configured specs mapper returned incompatible type while parsing '{typeof(TSource).Name}' into '{typeof(TSpecs).Name}'.");
        }

        public TSpecs Parse<TSpecs>(object source)
            => Parse<object, TSpecs>(source);

        public TSpecs Parse<TSource, TSpecs>(TSource source, TSpecs destination)
        {
            if (destination is null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            var mapped = Parse<TSource, TSpecs>(source);
            if (ReferenceEquals(mapped, destination))
            {
                return destination;
            }

            CopyPublicWritableProperties(mapped!, destination);
            return destination;
        }

        public object Parse(object source, Type sourceType, Type destinationType)
        {
            if (sourceType is null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            if (destinationType is null)
            {
                throw new ArgumentNullException(nameof(destinationType));
            }

            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (!sourceType.IsInstanceOfType(source))
            {
                throw new ArgumentException(
                    $"Source object is not assignable to declared source type '{sourceType.Name}'.",
                    nameof(source));
            }

            if (_options.Map is null)
            {
                throw new InvalidOperationException(
                    $"No specs mapper is configured to parse '{sourceType.Name}' into '{destinationType.Name}'.");
            }

            return _options.Map(_serviceProvider, source, destinationType)
                ?? throw new InvalidOperationException(
                    $"Configured specs mapper returned null while parsing '{sourceType.Name}' into '{destinationType.Name}'.");
        }

        public TTarget Apply<TSpecs, TTarget>(TTarget target, TSpecs specs)
        {
            var processor = _serviceProvider.GetRequiredService<IQuerySpecsProcessor<TSpecs, TTarget>>();
            processor.Validate(specs);
            return processor.Apply(target, specs);
        }

        private static void CopyPublicWritableProperties<TSource, TDestination>(TSource source, TDestination destination)
        {
            var sourceProperties = typeof(TSource)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.CanRead)
                .ToDictionary(x => x.Name, StringComparer.Ordinal);

            foreach (var destinationProperty in typeof(TDestination)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.CanWrite))
            {
                if (!sourceProperties.TryGetValue(destinationProperty.Name, out var sourceProperty))
                {
                    continue;
                }

                var value = sourceProperty.GetValue(source);
                destinationProperty.SetValue(destination, value);
            }
        }
    }
}
