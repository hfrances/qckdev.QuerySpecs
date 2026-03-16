using System.Collections.Generic;

namespace qckdev.QuerySpecs
{
    /// <summary>
    /// Internal contract for collection query communication across application boundaries.
    /// </summary>
    public class CollectionQuerySpecs
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
        public Dictionary<string, string?> Filters { get; set; } = new Dictionary<string, string?>();
    }
}
