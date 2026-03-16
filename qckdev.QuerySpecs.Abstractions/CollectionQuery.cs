using System.Collections.Generic;

namespace qckdev.QuerySpecs
{
    /// <summary>
    /// Base contract for collection queries with paging, search, ordering and dynamic filters.
    /// </summary>
    public class CollectionQuery
    {
        /// <summary>
        /// Gets or sets the optional page number (1-based).
        /// </summary>
        public int? Page { get; set; }

        /// <summary>
        /// Gets or sets the optional number of items per page.
        /// </summary>
        public int? PageSize { get; set; }

        /// <summary>
        /// Gets or sets the optional search text.
        /// </summary>
        public string? Search { get; set; }

        /// <summary>
        /// Gets or sets the optional ordering expression.
        /// </summary>
        public string? Order { get; set; }

        /// <summary>
        /// Gets or sets the optional dynamic filters.
        /// </summary>
        public Dictionary<string, string?>? Filters { get; set; }
    }
}
