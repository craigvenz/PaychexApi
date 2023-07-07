using System.Linq;

namespace Paychex.Api.Models.Companies
{
    /// <summary>
    /// Data elements of a Job Number to show the segments and number. For segmentation this can be up to 3 different segments.
    /// </summary>
    public class JobNumber
    {
        /// <summary>
        /// For EF to satisfy PK requirement. Currently not used.
        /// </summary>
        public System.Guid Id {get;set;}
        /// <summary>
        /// This is segment 1 or the number associated to the job when segmentation is not used.
        /// </summary>
        public string Segment1 { get; set; }
        /// <summary>
        /// This is segment 2.
        /// </summary>
        public string Segment2 { get; set; }
        /// <summary>
        /// This is segment 3.
        /// </summary>
        public string Segment3 { get; set; }

        /// <summary>
        /// Display formatting for JobNumber, obtained (tentatively, unsure if correct) from meeting with Paychex folks. 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => string.Join("-", new[] { Segment1, Segment2, Segment3 }.Where(s=>!string.IsNullOrEmpty(s)));  
    }
}
