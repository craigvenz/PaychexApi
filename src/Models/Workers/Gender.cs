// LcpMvc.Lcp.Paychex.Gender.cs
// Craig Venz - 01/15/2020 - 12:44 PM

using System.ComponentModel;

namespace Paychex.Api.Models.Workers {
    /// <summary>
    /// Gender types
    /// </summary>
    public enum Gender
    {
        /// <summary>
        /// </summary>
        [Description("Male")]
        MALE, 
        /// <summary>
        /// </summary>
        [Description("Female")]
        FEMALE, 
        /// <summary>
        /// </summary>
        [Description("Unknown")]
        UNKNOWN, 
        /// <summary>
        /// </summary>
        [Description("Not Specified")]
        NOT_SPECIFIED
    }
}