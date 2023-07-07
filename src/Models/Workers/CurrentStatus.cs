using System;
using Paychex.Api.Api;

namespace Paychex.Api.Models.Workers
{
    /// <summary>
    /// Status of the worker at this point in time.
    /// </summary>
    public class CurrentStatus
    {
        /// <summary>
        /// For EF to satisfy PK requirement. Currently not used.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// The unique identifier associated with this status representation.
        /// </summary>
        public string WorkerStatusId { get; set; }
        /// <summary>
        /// The workers current status.
        /// </summary>
        public WorkerStatusType StatusType { get; set; }
        /// <summary>
        /// The detailed reason of the workers current status.
        /// </summary>
        public WorkerStatusReason StatusReason { get; set; }
        /// <summary>
        /// Date that this status has started for the worker.
        /// </summary>
        public DateTime EffectiveDate { get; set; }
        /// <summary>
        /// debug info
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{StatusType.GetDescription()} because {StatusReason.GetDescription()} as of {EffectiveDate}";
    }
}
