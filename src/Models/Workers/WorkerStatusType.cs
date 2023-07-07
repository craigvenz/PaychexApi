// LcpMvc.Lcp.Paychex.WorkerStatusType.cs
// Craig Venz - 09/26/2019 - 11:14 AM

using System.ComponentModel;

namespace Paychex.Api.Models.Workers {
    /// <summary>
    /// The workers current status.
    /// </summary>
    public enum WorkerStatusType
    {
        /// <summary>
        /// </summary>
        [Description("Active")]
        ACTIVE,
        /// <summary>
        /// </summary>
        [Description("Inactive")]
        INACTIVE,
        /// <summary>
        /// </summary>
        [Description("Terminated")]
        TERMINATED,
        /// <summary>
        /// </summary>
        [Description("Transferred")]
        TRANSFERRED,
        /// <summary>
        /// </summary>
        [Description("Pending")]
        PENDING,
        /// <summary>
        /// </summary>
        [Description("In Progress")]
        IN_PROGRESS
    }
}