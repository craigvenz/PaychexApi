using System;
using System.Text;
using Paychex.Api.Models.Common;

namespace Paychex.Api.Models.Workers
{
    /// <summary>
    /// The state representation of allowed worker job titles configured for the company.
    /// </summary>
    public class JobTitle : BaseObject
    {
        /// <summary>
        /// The unique identifier associated with this job titles representation.
        /// </summary>
        public string JobTitleId { get; set; }
        /// <summary>
        /// Date when this job is available to be assigned to a worker.
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// Date when this job is no longer available to be assigned to a worker.
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// The name of the title.
        /// </summary>
        public string Title { get; set; }

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is JobTitle other && Equals(other);

        /// <summary>
        /// typed comparison shortcut
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        protected bool Equals(JobTitle other) => JobTitleId == other.JobTitleId;

        /// <inheritdoc />
        public override int GetHashCode() => (JobTitleId != null ? JobTitleId.GetHashCode() : 0);

        /// <summary>
        /// debug info
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            bool IsSet(DateTime? d) => d.GetValueOrDefault() != DateTime.MinValue;
            var sb = new StringBuilder();
            sb.AppendFormat("JobTitle: {0}", Title);
            if (IsSet(StartDate) && IsSet(EndDate))
            {
                sb.AppendFormat(
                    " {0}=>{1}",
                    StartDate.GetValueOrDefault()
                             .ToShortDateString(),
                    EndDate.GetValueOrDefault()
                           .ToShortDateString()
                );
            }
            else if (IsSet(StartDate))
            {
                sb.AppendFormat(
                    " {0}",
                    StartDate.GetValueOrDefault()
                             .ToShortDateString()
                );
            }

            return sb.ToString();
        } 
    }
}
