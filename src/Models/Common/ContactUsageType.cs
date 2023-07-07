using System.ComponentModel;

namespace Paychex.Api.Models.Common
{
    /// <summary>
    /// A code classifying a designated use associated with a contact method. For example, whether a telephone or email address is one
    /// for business communications or one primarily for personal use.
    /// </summary>
    public enum ContactUsageType
    {
        /// <summary>
        /// Business contact.
        /// </summary>
        [Description("Business Contact")]
        BUSINESS,
        /// <summary>
        /// Personal contact.
        /// </summary>
        [Description("Personal Contact")]
        PERSONAL
    }
}
