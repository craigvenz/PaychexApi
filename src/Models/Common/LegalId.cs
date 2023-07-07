namespace Paychex.Api.Models.Common
{
    /// <summary>
    /// Legal ID associated with an entity
    /// </summary>
    public class LegalId
    {
        /// <summary>
        /// type of legal id - valid values seen so far: SSN, FEIN
        /// </summary>
        public LegalIdType LegalIdType { get; set; }
        /// <summary>
        /// legal id value
        /// </summary>
        public string LegalIdValue { get; set; }

        /// <summary>
        /// Debug info
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{LegalIdType}: {LegalIdValue}";
    }
}
