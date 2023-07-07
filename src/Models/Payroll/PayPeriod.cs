using System;
using System.ComponentModel;

namespace Paychex.Api.Models.Payroll
{
    /// <summary>
    /// Represents a pay period, between two dates.
    /// </summary>
    public class PayPeriod
    {
        /// <summary>
        /// Paychex primary key id of the pay period.
        /// </summary>
        public string PayPeriodId { get; set; }

        /// <summary>
        /// Frequency of the payroll period.
        /// </summary>
        public enum IntervalCodes
        {
            /// <summary>
            /// None - unofficial interval type for when a pay period has no interval specified
            /// </summary>
            [Description("None")]
            NONE = 0,
            /// <summary>
            /// Annual
            /// </summary>
            [Description("Annual")]
            ANNUAL, 
            /// <summary>
            /// Bi-weekly
            /// </summary>
            [Description("Bi-Weekly")]
            BI_WEEKLY, 
            /// <summary>
            /// Monthly
            /// </summary>
            [Description("Monthly")]
            MONTHLY, 
            /// <summary>
            /// Quarterly
            /// </summary>
            [Description("Quarterly")]
            QUARTERLY, 
            /// <summary>
            /// Semi-annual
            /// </summary>
            [Description("Semi-Annual")]
            SEMI_ANNUAL, 
            /// <summary>
            /// Semi-monthly
            /// </summary>
            [Description("Semi-Monthly")]
            SEMI_MONTHLY, 
            /// <summary>
            /// Weekly
            /// </summary>
            [Description("Weekly")]
            WEEKLY
        }
        /// <summary>
        /// Interval of pay cycle
        /// We are only supporting WEEKLY.
        /// </summary>
        public IntervalCodes IntervalCode { get; set; }

        /// <summary>
        /// The current state of the associated pay period.
        /// </summary>
        public enum PayPeriodStatus
        {
            /// <summary>
            /// We will only use this status of pay periods.
            /// </summary>
            COMPLETED, 
            /// <summary>
            /// </summary>
            COMPLETED_BY_MEC, 
            /// <summary>
            /// </summary>
            ENTRY, 
            /// <summary>
            /// </summary>
            INITIAL, 
            /// <summary>
            /// </summary>
            PROCESSING, 
            /// <summary>
            /// </summary>
            REISSUED, 
            /// <summary>
            /// </summary>
            RELEASED, 
            /// <summary>
            /// </summary>
            REVERSED
        }
        /// <summary>
        /// Status of this payroll
        /// Only statuses of COMPLETED will be considered by us.
        /// </summary>
        public PayPeriodStatus Status { get; set; }
        /// <summary>
        /// User informational.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// first day of work period
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// last day of work period. Mapped to our week end date.
        /// </summary>
        public DateTime EndDate { get; set; }
        /// <summary>
        /// Submit by date. Not used by us.
        /// </summary>
        public DateTime SubmitByDate { get; set; }
        /// <summary>
        /// Check date. Not used by us.
        /// </summary>
        public DateTime CheckDate { get; set; }
        /// <summary>
        /// number of checks in the payroll period.
        /// </summary>
        public int CheckCount { get; set; }

        /// <summary>
        /// ToString override - debug info
        /// </summary>
        /// <returns>String representation for debugging</returns>
        public override string ToString() =>
            $"PayPeriod {{Id:{PayPeriodId}  {StartDate.ToShortDateString()}=>{EndDate.ToShortDateString()}, {CheckCount} checks}}";
    }
}
