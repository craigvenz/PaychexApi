// LcpMvc.Lcp.Paychex.EmployeeExemptionType.cs
// Craig Venz - 01/15/2020 - 11:45 AM

using System.ComponentModel;

namespace Paychex.Api.Models.Workers {
    /// <summary>
    /// The Overtime classification of the worker. 
    /// </summary>
    public enum EmployeeExemptionType
    {
        /// <summary>
        /// </summary>
        [Description("Exempt")]
        EXEMPT, 
        /// <summary>
        /// </summary>
        [Description("Non Exempt")]
        NON_EXEMPT
    }
}