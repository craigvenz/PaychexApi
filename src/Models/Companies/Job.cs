using System;
using Paychex.Api.Models.Common;

namespace Paychex.Api.Models.Companies
{
    /// <summary>
    /// Seems to refer to a Paychex Project.
    /// </summary>
    public class Job : BaseObject
    {
        /// <summary>
        /// paychex internal primary key id for this job
        /// </summary>
        public string JobId { get; set; }
        /// <summary>
        /// name of the job. Can be null.
        /// </summary>
        public string JobName { get; set; }
        /// <summary>
        /// Start date of the job.
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// End date of the job.
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// Job number - use this to identify project code if the name is null. <see cref="Companies.JobNumber"/>
        /// </summary>
        public JobNumber JobNumber { get; set; }

        /// <summary>
        /// If jobName is empty, use jobNumber
        /// </summary>
        /// <returns></returns>
        public override string ToString() =>
            (!string.IsNullOrEmpty(JobName) ? JobName : JobNumber?.ToString()) ?? string.Empty;

        /// <inheritdoc />
        public override bool Equals(object obj) =>
            obj is Job other && 
            other.JobId.Equals(JobId, StringComparison.InvariantCultureIgnoreCase);

        /// <inheritdoc />
        public override int GetHashCode() => JobId.GetHashCode();
    }
}
