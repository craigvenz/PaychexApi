using System.ComponentModel;

namespace Paychex.Api.Models.Common
{
    /// <summary>
    /// A set of communication types classifying an instruction that the customer, requester, or subject must comply with in order for the screening to go forward.
    /// </summary>
    public enum ContactType
    {
        /// <summary>
        /// Phone number
        /// </summary>
        [Description("Phone Number")]
        PHONE, 
        /// <summary>
        /// Email address
        /// </summary>
        [Description("Email Address")]
        EMAIL, 
        /// <summary>
        /// Street address
        /// </summary>
        [Description("Address")]
        STREET_ADDRESS,
        /// <summary>
        /// PO box
        /// </summary>
        [Description("Post Office Box")]
        PO_BOX_ADDRESS,
        /// <summary>
        /// Mobile phone
        /// </summary>
        [Description("Mobile Phone")]
        MOBILE_PHONE
    }
}
