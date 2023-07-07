namespace Paychex.Api.Models.Companies
{
    /// <summary>
    /// Unsure of usage and usefulness.
    /// </summary>
    public class PayComponentFrequency
    {
        /// <summary>
        /// For EF to satisfy PK requirement. Currently not used.
        /// </summary>
        public System.Guid Id {get;set;}
        /// <summary>
        /// From api documentation: Currently we only support a BY_PAY_PERIOD value for the API.
        /// </summary>
        public string Applied { get; set; }
        /// <summary>
        /// This is how often the to be applied on the pay run.
        /// </summary>
        public string Occurrence { get; set; }
        /// <summary>
        /// These are sub intervals that are used for occurrences that are larger in duration that allow you to define when to apply the pay component.
        /// </summary>
        public OccurrenceIntervals OccurrenceIntervals { get; set; }
    }
}
