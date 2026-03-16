using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using qckdev.QuerySpecs;
using System;
using System.Linq;
using System.Reflection;

namespace qckdev.QuerySpecs.Test
{
    [TestClass]
    public class QuerySpecsEngineTests
    {
        [TestMethod]
        public void Parse_UsesConfiguredReflectionDelegate()
        {
            using var sp = new ServiceCollection()
                .AddQuerySpecs(config =>
                {
                    config.Map = (_, source, destinationType) => ReflectionMapper.MapByName(source, destinationType);
                })
                .BuildServiceProvider();

            using var scope = sp.CreateScope();
            var options = scope.ServiceProvider.GetRequiredService<IOptions<QuerySpecsEngineOptions>>();
            Assert.IsNotNull(options.Value.Map);

            var sut = scope.ServiceProvider.GetRequiredService<IQuerySpecsEngine>();
            var source = new Source { Name = "demo", Number = 7 };

            var result = sut.Parse<Source, Destination>(source);

            Assert.AreEqual("demo", result.Name);
            Assert.AreEqual(7, result.Number);
        }

        [TestMethod]
        public void ParseObjectOverload_UsesConfiguredMapper()
        {
            using var sp = new ServiceCollection()
                .AddQuerySpecs(config =>
                {
                    config.Map = (_, source, destinationType) => ReflectionMapper.MapByName(source, destinationType);
                })
                .BuildServiceProvider();

            using var scope = sp.CreateScope();
            var sut = scope.ServiceProvider.GetRequiredService<IQuerySpecsEngine>();

            var result = sut.Parse<Destination>(new Source { Name = "object-overload", Number = 9 });

            Assert.AreEqual("object-overload", result.Name);
            Assert.AreEqual(9, result.Number);
        }

        [TestMethod]
        public void ParseIntoExistingDestination_UpdatesInstance()
        {
            using var sp = new ServiceCollection()
                .AddQuerySpecs(config =>
                {
                    config.Map = (_, source, destinationType) => ReflectionMapper.MapByName(source, destinationType);
                })
                .BuildServiceProvider();

            using var scope = sp.CreateScope();
            var sut = scope.ServiceProvider.GetRequiredService<IQuerySpecsEngine>();

            var destination = new Destination { Name = "original", Number = 0 };
            var result = sut.Parse(new Source { Name = "updated", Number = 15 }, destination);

            Assert.AreSame(destination, result);
            Assert.AreEqual("updated", destination.Name);
            Assert.AreEqual(15, destination.Number);
        }

        [TestMethod]
        public void ParseRuntimeOverload_Maps_WhenConfigurationIsValid()
        {
            using var sp = new ServiceCollection()
                .AddQuerySpecs(config =>
                {
                    config.Map = (_, source, destinationType) => ReflectionMapper.MapByName(source, destinationType);
                })
                .BuildServiceProvider();

            using var scope = sp.CreateScope();
            var sut = scope.ServiceProvider.GetRequiredService<IQuerySpecsEngine>();

            var result = sut.Parse(new Source { Name = "runtime", Number = 99 }, typeof(Source), typeof(Destination));
            var typed = (Destination)result;

            Assert.AreEqual("runtime", typed.Name);
            Assert.AreEqual(99, typed.Number);
        }

        [TestMethod]
        public void Parse_Throws_WhenMapIsNotConfigured()
        {
            using var sp = new ServiceCollection()
                .AddQuerySpecs(config => config.RegisterServicesFromAssemblyContaining<IncrementQuerySpecsProcessor>())
                .BuildServiceProvider();

            using var scope = sp.CreateScope();
            var sut = scope.ServiceProvider.GetRequiredService<IQuerySpecsEngine>();

            var ex = Assert.ThrowsException<InvalidOperationException>(() =>
                sut.Parse<Source, Destination>(new Source { Name = "no-map", Number = 1 }));

            StringAssert.Contains(ex.Message, "No specs mapper is configured");
        }

        [TestMethod]
        public void ParseRuntimeOverload_Throws_WhenSourceIsNull()
        {
            using var sp = new ServiceCollection()
                .AddQuerySpecs(config =>
                {
                    config.Map = (_, source, destinationType) => ReflectionMapper.MapByName(source, destinationType);
                })
                .BuildServiceProvider();

            using var scope = sp.CreateScope();
            var sut = scope.ServiceProvider.GetRequiredService<IQuerySpecsEngine>();

            Assert.ThrowsException<ArgumentNullException>(() =>
                sut.Parse(source: null!, sourceType: typeof(Source), destinationType: typeof(Destination)));
        }

        [TestMethod]
        public void ParseRuntimeOverload_Throws_WhenSourceTypeIsIncompatible()
        {
            using var sp = new ServiceCollection()
                .AddQuerySpecs(config =>
                {
                    config.Map = (_, source, destinationType) => ReflectionMapper.MapByName(source, destinationType);
                })
                .BuildServiceProvider();

            using var scope = sp.CreateScope();
            var sut = scope.ServiceProvider.GetRequiredService<IQuerySpecsEngine>();

            var ex = Assert.ThrowsException<ArgumentException>(() =>
                sut.Parse(new Source(), typeof(Destination), typeof(Destination)));

            StringAssert.Contains(ex.Message, "not assignable");
        }

        [TestMethod]
        public void ParseRuntimeOverload_Throws_WhenMapReturnsNull()
        {
            using var sp = new ServiceCollection()
                .AddQuerySpecs(config =>
                {
                    config.Map = (serviceProvider, source, destinationType) => null;
                })
                .BuildServiceProvider();

            using var scope = sp.CreateScope();
            var sut = scope.ServiceProvider.GetRequiredService<IQuerySpecsEngine>();

            var ex = Assert.ThrowsException<InvalidOperationException>(() =>
                sut.Parse(new Source(), typeof(Source), typeof(Destination)));

            StringAssert.Contains(ex.Message, "returned null");
        }

        [TestMethod]
        public void Apply_ResolvesHandlerRegisteredFromAssembly()
        {
            using var sp = new ServiceCollection()
                .AddQuerySpecs(config =>
                {
                    config.RegisterServicesFromAssemblyContaining<IncrementQuerySpecsProcessor>();
                })
                .BuildServiceProvider();

            using var scope = sp.CreateScope();
            var sut = scope.ServiceProvider.GetRequiredService<IQuerySpecsEngine>();

            var result = sut.Apply<int, int>(3, 2);

            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public void AddQuerySpecs_RegistersProcessorForEachClosedContract()
        {
            using var sp = new ServiceCollection()
                .AddQuerySpecs(config =>
                {
                    config.RegisterServicesFromAssemblyContaining<MultiProcessor>();
                })
                .BuildServiceProvider();

            using var scope = sp.CreateScope();
            var first = scope.ServiceProvider.GetRequiredService<IQuerySpecsProcessor<int, int>>();
            var second = scope.ServiceProvider.GetRequiredService<IQuerySpecsProcessor<string, string>>();

            Assert.IsInstanceOfType(first, typeof(MultiProcessor));
            Assert.IsInstanceOfType(second, typeof(MultiProcessor));
        }

        [TestMethod]
        public void CollectionQuerySpecs_HasExpectedDefaults()
        {
            var specs = new CollectionQuerySpecs();

            Assert.IsNotNull(specs.Filters);
            Assert.AreEqual(0, specs.Filters.Count);
        }

        sealed class Source
        {
            public string Name { get; set; } = string.Empty;
            public int Number { get; set; }
        }

        sealed class Destination
        {
            public string Name { get; set; } = string.Empty;
            public int Number { get; set; }
        }

        sealed class IncrementQuerySpecsProcessor : IQuerySpecsProcessor<int, int>
        {
            public int Apply(int target, int specs) => target + specs;
        }

        sealed class MultiProcessor : IQuerySpecsProcessor<int, int>, IQuerySpecsProcessor<string, string>
        {
            int IQuerySpecsProcessor<int, int>.Apply(int target, int specs) => target + specs;
            string IQuerySpecsProcessor<string, string>.Apply(string target, string specs) => target + specs;
        }

        static class ReflectionMapper
        {
            public static object MapByName(object source, Type destinationType)
            {
                var destination = Activator.CreateInstance(destinationType)
                    ?? throw new InvalidOperationException($"Could not create '{destinationType.Name}'.");

                var sourceProperties = source.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(x => x.CanRead)
                    .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

                foreach (var destinationProperty in destinationType
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(x => x.CanWrite))
                {
                    if (!sourceProperties.TryGetValue(destinationProperty.Name, out var sourceProperty))
                    {
                        continue;
                    }

                    var value = sourceProperty.GetValue(source);
                    destinationProperty.SetValue(destination, value);
                }

                return destination;
            }
        }
    }
}
