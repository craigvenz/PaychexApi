namespace Paychex.Api.Models.Companies
{
    /// <summary>
    /// These are sub intervals that are used for occurrences that are larger in duration that allow you to define when to apply the pay component.
    /// </summary>
    public class OccurrenceIntervals
    {
        /// <summary>
        /// For EF to satisfy PK requirement. Currently not used.
        /// </summary>
        public System.Guid Id {get;set;}
        /// <summary>
        /// Not documented in Paychex api documentation
        /// </summary>
        public string Interval1 { get; set; }
        /// <summary>
        /// Not documented in Paychex api documentation
        /// </summary>
        public string Interval2 { get; set; }
    }
}
