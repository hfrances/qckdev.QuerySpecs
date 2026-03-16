namespace qckdev.QuerySpecs
{
    /// <summary>
    /// Applies and validates a query specs contract over a target object.
    /// </summary>
    /// <typeparam name="TSpecs">Specs input type.</typeparam>
    /// <typeparam name="TTarget">Target type to apply specs on.</typeparam>
    public interface IQuerySpecsProcessor<in TSpecs, TTarget>
    {
        /// <summary>
        /// Validates the provided specs and applies it over the target instance.
        /// </summary>
        /// <param name="target">Target object.</param>
        /// <param name="specs">Specs input instance.</param>
        /// <returns>Transformed target object.</returns>
        TTarget Apply(TTarget target, TSpecs specs);
    }
}
