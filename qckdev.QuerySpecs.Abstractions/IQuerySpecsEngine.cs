using System;

namespace qckdev.QuerySpecs
{
    /// <summary>
    /// Resolves, parses and executes query specs processors by generic types.
    /// </summary>
    public interface IQuerySpecsEngine
    {
        /// <summary>
        /// Parses a source object into a query specs object.
        /// </summary>
        /// <typeparam name="TSource">Source input type.</typeparam>
        /// <typeparam name="TSpecs">Destination specs type.</typeparam>
        /// <param name="source">Source input instance.</param>
        /// <returns>Parsed query specs.</returns>
        TSpecs Parse<TSource, TSpecs>(TSource source);

        /// <summary>
        /// Parses a source object into a destination specs type.
        /// </summary>
        /// <typeparam name="TSpecs">Destination specs type.</typeparam>
        /// <param name="source">Source input instance.</param>
        /// <returns>Parsed query specs.</returns>
        TSpecs Parse<TSpecs>(object source);

        /// <summary>
        /// Parses a source object into the provided destination instance.
        /// </summary>
        /// <typeparam name="TSource">Source input type.</typeparam>
        /// <typeparam name="TSpecs">Destination specs type.</typeparam>
        /// <param name="source">Source input instance.</param>
        /// <param name="destination">Destination instance to populate.</param>
        /// <returns>The populated destination instance.</returns>
        TSpecs Parse<TSource, TSpecs>(TSource source, TSpecs destination);

        /// <summary>
        /// Parses a source object into a destination specs type using runtime types.
        /// </summary>
        /// <param name="source">Source input instance.</param>
        /// <param name="sourceType">Runtime source type.</param>
        /// <param name="destinationType">Runtime destination type.</param>
        /// <returns>Parsed destination object.</returns>
        object Parse(object source, Type sourceType, Type destinationType);

        /// <summary>
        /// Applies specs over the provided target using the registered query specs processor.
        /// </summary>
        /// <typeparam name="TSpecs">Specs input type.</typeparam>
        /// <typeparam name="TTarget">Target type.</typeparam>
        /// <param name="target">Target instance.</param>
        /// <param name="specs">Specs input.</param>
        /// <returns>Transformed target instance.</returns>
        TTarget Apply<TSpecs, TTarget>(TTarget target, TSpecs specs);
    }
}
