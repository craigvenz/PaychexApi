using Paychex.Api.Models.Common;
using Paychex.Api.Models.Companies;

namespace Paychex.Api.Models.Payroll
{
    /// <summary>
    /// Taxation information
    /// </summary>
    public class Tax
    {
        /// <summary>
        /// For EF to satisfy PK requirement. Currently not used.
        /// </summary>
        public System.Guid Id { get; set; }
        /// <summary>
        /// Name of tax.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Associated <see cref="LaborAssignment">labor assignment</see>
        /// </summary>
        public string LaborAssignmentId { get; set; }
        /// <summary>
        /// Associated <see cref="Companies.Job">job</see>
        /// </summary>
        public string JobId { get; set; }
        /// <summary>
        /// Associated <see cref="Companies.Job">job name. Apparently added and supported now?</see>
        /// </summary>
        public string JobName { get; set; }
        /// <summary>
        /// Navigation property for labor assignment
        /// </summary>
        [JsonIgnore]
        public LaborAssignment Assignment { get; set; }
        /// <summary>
        /// Navigation property for job
        /// </summary>
        [JsonIgnore]
        public Job Job { get; set; }
        /// <summary>
        /// Tax paid by 
        /// </summary>
        public enum TaxPaidBy
        {
            /// <summary>
            /// 
            /// </summary>
            EMPLOYEE_WITHHOLDING,
            /// <summary>
            /// 
            /// </summary>
            EMPLOYER_LIABILITY
        }
        /// <summary>
        /// Pre or post tax applied?  Values: EMPLOYEE_WITHHOLDING, EMPLOYER_LIABILITY 
        /// </summary>
        public TaxPaidBy PaidBy { get; set; }
        /// <summary>
        /// Dollar amount paid
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// debug info
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{Name}, {Amount:c} - paid by {PaidBy}, {Job}, {Assignment}";
    }
}
