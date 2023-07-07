using System;
using System.Collections.Generic;
using System.Text;
using Paychex.Api.Api;
using Paychex.Api.Models.Common;
using Paychex.Api.Models.Payroll;

namespace Paychex.Api.Models.Workers
{
    /// <summary>
    /// https://developer.paychex.com/api-documentation-and-exploration/api-references/workers
    /// The representational state of a person who is classified as a worker. A worker is an exempt or non-exempt employee or contractor.
    /// </summary>
    public class Worker : BaseObject
    {
        /// <summary>
        /// The unique identifier associated with this worker representation.
        /// </summary>
        public string WorkerId { get; set; }
        /// <summary>
        /// The workers employee identification information.
        /// </summary>
        public string EmployeeId { get; set; }
        /// <summary>
        /// The type of worker.
        /// </summary>
        public WorkerType WorkerType { get; set; }
        /// <summary>
        /// The type of employment for the worker. 
        /// </summary>
        public WorkerEmploymentType EmploymentType { get; set; }
        /// <summary>
        /// The Overtime classification of the worker.
        /// </summary>
        public EmployeeExemptionType ExemptionType { get; set; }
        /// <summary>
        /// State in which the worker is doing work?
        /// </summary>
        public string WorkState { get; set; }
        /// <summary>
        /// The workers Date of Birth.
        /// </summary>
        public DateTime BirthDate { get; set; }
        /// <summary>
        /// Worker's gender.
        /// </summary>
        public Gender Sex { get; set; }
        /// <summary>
        /// A code classifying a person on the basis of their ethnicity.
        /// </summary>
        public EthnicityType EthnicityCode { get; set; }
        /// <summary>
        /// The date which the worker was hired.
        /// </summary>
        public DateTime? HireDate { get; set; }
        /// <summary>
        /// Information about the workers name. See <see cref="Workers.Name">Name</see>.
        /// </summary>
        public Name Name { get; set; }
        /// <summary>
        /// The workers legal tax identification information - SSN usually. See <see cref="Common.LegalId">LegalId</see>.
        /// </summary>
        public LegalId LegalId { get; set; }
        /// <summary>
        /// Get SSN
        /// </summary>
        /// <returns></returns>
        public string GetLegalId() =>
            LegalId?.LegalIdType == LegalIdType.SSN || LegalId?.LegalIdType == LegalIdType.SSNLast4
                ? LegalId.LegalIdValue
                : string.Empty;
        /// <summary>
        /// The workers location.
        /// </summary>
        public string LocationId { get; set; }
        /// <summary>
        /// The workers home labor assignment.
        /// </summary>
        public string LaborAssignmentId { get; set; }
        /// <summary>
        /// Navigation property for the worker's home assignment - used by me for getting default when
        /// a check doesn't list one
        /// </summary>
        [JsonIgnore]
        public LaborAssignment HomeAssignment {get;set;}
        /// <summary>
        /// The workers home job.
        /// </summary>
        public string JobId { get; set; }
        /// <summary>
        /// Navigation property for the worker's home job - used by me for getting default when
        /// a check doesn't list one
        /// </summary>
        [JsonIgnore]
        public Companies.Job HomeJob {get;set;}
        /// <summary>
        /// The organization which the worker is part of.
        /// </summary>
        public Organization Organization { get; set; }
        /// <summary>
        /// Worker's supervisor.
        /// </summary>
        public Supervisor Supervisor { get; set; }
        /// <summary>
        /// The workers employment <see cref="Workers.CurrentStatus">status information</see>.
        /// </summary>
        public CurrentStatus CurrentStatus { get; set; }
        /// <summary>
        /// Information about a workers communications.
        /// </summary>
        public List<Communication> Communications { get; set; }
        /// <summary>
        /// Worker's job title
        /// </summary>
        public JobTitle Job { get; set; }
        /// <summary>
        /// Unknown
        /// </summary>
        public string ClockId { get; set; }
        /// <inheritdoc />
        public override bool Equals(object obj) => obj is Worker other && Equals(other);
        /// <summary>
        /// Equality comparer, just use ids.
        /// </summary>
        /// <param name="other">comparison object</param>
        /// <returns>true if worker ids are equal</returns>
        protected bool Equals(Worker other) => WorkerId == other.WorkerId;
        /// <inheritdoc />
        public override int GetHashCode() => (WorkerId != null ? WorkerId.GetHashCode() : 0);
        /// <summary>
        /// debug info
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat(
                "employeeId:{0} - [{1}] {2} ({3}) {4}, {5} {6}, {7}\n",
                EmployeeId,
                LegalId,
                Name,
                Sex.GetDescription(),
                EthnicityCode.GetDescription(),
                EmploymentType.GetDescription(),
                WorkerType.GetDescription(),
                CurrentStatus
            );

            return sb.ToString();
        } 
        
    }
}
