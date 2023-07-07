// LcpMvc.Lcp.Paychex.WorkerEmploymentType.cs
// Craig Venz - 01/15/2020 - 12:44 PM

using System.ComponentModel;

namespace Paychex.Api.Models.Workers {
    /// <summary>
    /// The type of employment for the worker.
    /// </summary>
    public enum WorkerEmploymentType
    {
        /// <summary>
        /// </summary>
        [Description("Full Time")]
        FULL_TIME, 
        /// <summary>
        /// </summary>
        [Description("Part Time")]
        PART_TIME
    }
}