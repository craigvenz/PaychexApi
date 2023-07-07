namespace Paychex.Api.Models.Common
{
    /// <summary>
    /// Extra information returned by the API
    /// </summary>
    public class Metadata
    {
        /// <summary>
        /// Total number of items that could be retrieved by this call
        /// </summary>
        public int ContentItemCount { get; set; }
        /// <summary>
        /// Object with paging parameters. <see cref="Common.Pagination"/>
        /// </summary>
        public Pagination Pagination { get; set; }
        /// <summary>
        /// Debug info
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{ContentItemCount} items  {Pagination}";
    }
}
