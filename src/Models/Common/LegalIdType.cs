using System.ComponentModel;

namespace Paychex.Api.Models.Common
{
    /// <summary>
    /// A value that identifies the type of taxpayer identification number provided.
    /// </summary>
    public enum LegalIdType
    {
        /// <summary>
        /// Social Security Number - 9 digit number
        /// </summary>
        [Description("Social Security Number")]
        SSN, 
        /// <summary>
        /// Federal Employer Identification Number (EIN)
        /// </summary>
        [Description("Federal Employer Identification Number")]
        FEIN, 
        /// <summary>
        /// Social Insurance Number
        /// </summary>
        [Description("Social Insurance Number")]
        SIN, 
        /// <summary>
        /// Last 4 digits of the Social Security Number
        /// </summary>
        [Description("Last 4 digits of the Social Security Number")]
        SSNLast4
    }
}
