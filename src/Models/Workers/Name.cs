namespace Paychex.Api.Models.Workers
{
    /// <summary>
    /// Data elements of a workers name.
    /// </summary>
    public class Name
    {
        /// <summary>
        /// For EF to satisfy PK requirement. Currently not used.
        /// </summary>
        public System.Guid Id { get; set; }
        /// <summary>
        /// The family or last name of a worker.
        /// </summary>
        public string FamilyName { get; set; }
        /// <summary>
        /// A subordinate given name, or initial representing that name, of a worker.
        /// </summary>
        public string MiddleName { get; set; }
        /// <summary>
        /// The given or first name of a worker. For an independent contractor that is a company rather than an individual, the name of the company.
        /// </summary>
        public string GivenName { get; set; }
        /// <summary>
        /// A title indicating rank (Sargent), profession (Reverend, Doctor), or marital status (Miss, Mrs) used by a worker.
        /// </summary>
        public string TitleAffixCode { get; set; }
        /// <summary>
        /// A qualifier to the name of a worker, indicating generation.
        /// </summary>
        public string QualificationAffixCode { get; set; }
        /// <summary>
        /// The name that the worker prefers to go by; also known as a "nickname"
        /// </summary>
        public string PreferredName {get;set;}

        /// <summary>
        /// Show debug info through ToString
        /// </summary>
        /// <returns>string</returns>
        public override string ToString() => $"{GivenName} {FamilyName}";
    }
}
