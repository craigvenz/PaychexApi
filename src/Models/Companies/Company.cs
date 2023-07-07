using System.Collections.Generic;
using Paychex.Api.Models.Common;

namespace Paychex.Api.Models.Companies
{
    /// <summary>
    /// Represents a Company associated with an account. Seems to be analogous to our Contractor entity.
    /// </summary>
    public class Company : BaseObject
    {
        /// <summary>
        /// Paychex internal primary key ID for this company
        /// </summary>
        public string CompanyId { get; set; }
        /// <summary>
        /// display id for this company. Found in the paychex web app.
        /// </summary>
        public string DisplayId { get; set; }
        /// <summary>
        /// Company name.
        /// </summary>
        public string LegalName { get; set; }
        /// <summary>
        /// Federal Employer Id of the company. <see cref="Common.LegalId"/>
        /// </summary>
        public LegalId LegalId { get; set; }
        /// <summary>
        /// List of contacts for the company. <see cref="Communication"/>
        /// </summary>
        public List<Communication> Communications { get; set; }
    }
}
