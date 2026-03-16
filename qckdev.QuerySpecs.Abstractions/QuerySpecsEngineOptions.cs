using System;

namespace qckdev.QuerySpecs
{
    /// <summary>
    /// Configuration for parse behaviors used by <see cref="IQuerySpecsEngine"/>.
    /// </summary>
    public sealed class QuerySpecsEngineOptions
    {
        /// <summary>
        /// Optional mapping delegate used to transform an input object to a destination type.
        /// Signature: (serviceProvider, source, destinationType) -&gt; mapped object.
        /// </summary>
        public Func<IServiceProvider, object, Type, object?>? Map { get; set; }
    }
}
