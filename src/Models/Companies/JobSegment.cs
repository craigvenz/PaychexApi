namespace Paychex.Api.Models.Companies
{
    /// <summary>
    /// More information about a job segment. Don't really know what this is used for - I assume links to JobNumber.segment1/2/3?
    /// Informed by Paychex on 12/20/2019 that this is probably going to be have to be handled for the
    /// weird customers who do use it.
    /// From Api docs: The response will give you back the exact character lengths and the segment names so you are able to POST.
    /// </summary>
    public class JobSegment
    {
        /// <summary>
        /// PK id for EF. Not used.
        /// </summary>
        public System.Guid Id {get;set;}
        /// <summary>
        /// Number of segment - 1, 2, or 3
        /// </summary>
        public int SegmentNumber { get; set; }
        /// <summary>
        /// Name of segment
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Number of characters in segment
        /// </summary>
        public int SegmentLength { get; set; }
    }
}
