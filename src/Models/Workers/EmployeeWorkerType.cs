// LcpMvc.Lcp.Paychex.EmployeeWorkerType.cs
// Craig Venz - 01/15/2020 - 12:44 PM

using System.ComponentModel;

namespace Paychex.Api.Models.Workers {
    /// <summary>
    /// The type of worker. 
    /// </summary>
    public enum EmployeeWorkerType
    {
        /// <summary>
        /// </summary>
        [Description("Employee")]
        EMPLOYEE,
        /// <summary>
        /// </summary>
        [Description("Contractor")]
        CONTRACTOR, 
        /// <summary>
        /// </summary>
        [Description("Independent Contractor")]
        INDEPENDENT_CONTRACTOR
    }
}