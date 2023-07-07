namespace Paychex.Api.Models.Companies
{
    /// <summary>
    /// Unsure of usage and usefulness at this time.
    /// The state representation of generic pay frequencies.
    /// </summary>
    public class PayFrequencies
    {
        /// <summary>
        /// For EF to satisfy PK requirement. Currently not used.
        /// </summary>
        public System.Guid Id {get;set;}
        /// <summary>
        /// Frequency which the pay component would be applied.
        /// </summary>
        public string PayFrequency { get; set; }
        /// <summary>
        /// Frequency details. See <see cref="Companies.PayComponentFrequency">PayComponentFrequency</see>
        /// </summary>
        public PayComponentFrequency PayComponentFrequency { get; set; }
    }
}
