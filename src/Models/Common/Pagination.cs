namespace Paychex.Api.Models.Common
{
    /// <summary>
    /// Paging support for API calls
    /// </summary>
    public class Pagination
    {
        /// <summary>
        /// Number of records per page
        /// </summary>
        public const int DefaultPageSize = 50;

        /// <summary>
        /// Number of records to skip
        /// </summary>
        public int? Offset { get; set; }

        /// <summary>
        /// Number of records to retrieve
        /// </summary>
        public int? Limit { get; set; }

        /// <summary>
        /// Actual number of items returned
        /// </summary>
        public int ItemCount { get; set; }

        /// <summary>
        /// ETag value - used internally by API
        /// </summary>
        [JsonIgnore]
        public string ETag { get; set; }

        /// <summary>
        /// debug info
        /// </summary>
        /// <returns>string of debug info</returns>
        public override string ToString() => $"itemCount {ItemCount}, offset {Offset}, limit {Limit}";

        /// <summary>
        /// Static empty declaration of pagination class
        /// </summary>
        public static Pagination Empty = new Pagination
        {
            Offset = 0,
            Limit = 0,
            ItemCount = 0
        };
    }
}
