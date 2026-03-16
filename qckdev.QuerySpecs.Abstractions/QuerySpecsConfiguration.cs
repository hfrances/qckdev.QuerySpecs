using System;
using System.Collections.Generic;
using System.Reflection;

namespace qckdev.QuerySpecs
{
    /// <summary>
    /// Configuration for query specs registration and parse delegates.
    /// </summary>
    public sealed class QuerySpecsConfiguration
    {
        private readonly HashSet<Assembly> _assemblies = new HashSet<Assembly>();

        /// <summary>
        /// Optional mapping delegate used to transform an input object to a destination type.
        /// Signature: (serviceProvider, source, destinationType) -&gt; mapped object.
        /// </summary>
        public Func<IServiceProvider, object, Type, object?>? Map { get; set; }

        /// <summary>
        /// Assemblies selected for processor discovery.
        /// </summary>
        public IReadOnlyCollection<Assembly> AssembliesToRegister => _assemblies;

        /// <summary>
        /// Registers query specs processors from one assembly.
        /// </summary>
        /// <param name="assembly">Assembly to scan.</param>
        public void RegisterServicesFromAssembly(Assembly assembly)
        {
            if (assembly is null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            _assemblies.Add(assembly);
        }

        /// <summary>
        /// Registers query specs processors from many assemblies.
        /// </summary>
        /// <param name="assemblies">Assemblies to scan.</param>
        public void RegisterServicesFromAssemblies(params Assembly[] assemblies)
            => RegisterServicesFromAssemblies((IEnumerable<Assembly>)assemblies);

        /// <summary>
        /// Registers query specs processors from many assemblies.
        /// </summary>
        /// <param name="assemblies">Assemblies to scan.</param>
        public void RegisterServicesFromAssemblies(IEnumerable<Assembly> assemblies)
        {
            if (assemblies is null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }

            foreach (var assembly in assemblies)
            {
                RegisterServicesFromAssembly(assembly);
            }
        }

        /// <summary>
        /// Registers query specs processors from the assembly containing <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">A type from the target assembly.</typeparam>
        public void RegisterServicesFromAssemblyContaining<T>()
            => RegisterServicesFromAssembly(typeof(T).Assembly);

        /// <summary>
        /// Registers query specs processors from the assembly containing a type.
        /// </summary>
        /// <param name="type">A type from the target assembly.</param>
        public void RegisterServicesFromAssemblyContaining(Type type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            RegisterServicesFromAssembly(type.Assembly);
        }
    }
}
