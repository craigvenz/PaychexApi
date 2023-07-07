// LcpMvc.Lcp.Paychex.EthnicityType.cs
// Craig Venz - 01/15/2020 - 11:45 AM

using System.ComponentModel;

namespace Paychex.Api.Models.Workers {
    /// <summary>
    /// A code classifying a person on the basis of their ethnicity.
    /// </summary>
    public enum EthnicityType 
    {
        /// <summary>
        /// </summary>
        [Description("Hispanic or Latino")]
        HISPANIC_OR_LATINO,
        /// <summary>
        /// </summary>
        [Description("Caucasian")]
        WHITE_NOT_OF_HISPANIC_ORIGIN,
        /// <summary>
        /// </summary>
        [Description("African American")]
        BLACK_OR_AFRICAN_AMERICAN,
        /// <summary>
        /// </summary>
        [Description("Hawaiian or Pacific Islander")]
        NATIVE_HAWAIIAN_OR_OTHER_PACIFIC_ISLAND,
        /// <summary>
        /// </summary>
        [Description("American Indian or Alaskan Native")]
        AMERICAN_INDIAN_OR_ALASKAN_NATIVE,
        /// <summary>
        /// </summary>
        [Description("Two or more Races")]
        TWO_OR_MORE_RACES,
        /// <summary>
        /// </summary>
        [Description("Asian or Pacific Islander")]
        ASIAN_OR_PACIFIC_ISLANDER,
        /// <summary>
        /// </summary>
        [Description("Asian")]
        ASIAN
    }
}