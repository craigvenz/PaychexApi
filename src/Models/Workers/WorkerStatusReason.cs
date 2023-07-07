// LcpMvc.Lcp.Paychex.WorkerStatusReason.cs
// Craig Venz - 09/26/2019 - 11:14 AM

using System.ComponentModel;

namespace Paychex.Api.Models.Workers {
    /// <summary>
    /// The detailed reason of the workers current status.
    /// </summary>
    public enum WorkerStatusReason
    {
        /// <summary>
        /// Applies to Active status only
        /// </summary>
        [Description("Hired")]
        HIRED,
        /// <summary>
        /// Applies to Active status only
        /// </summary>
        [Description("Returned to work")]
        RETURN_TO_WORK,
        /// <summary>
        /// Applies to Active status only
        /// </summary>
        [Description("Rehired")]
        REHIRED,
        /// <summary>
        /// Applies to Active status only
        /// </summary>
        ACTIVATE_EMP,
        /// <summary>
        /// Applies to Active status only
        /// </summary>
        [Description("Began Contract")]
        BEGIN_CONTRACT,
        /// <summary>
        /// Applies to Active status only
        /// </summary>
        [Description("Resumed Contract")]
        RESUME_CONTRACT,
        /// <summary>
        /// Applies to Active status only
        /// </summary>
        [Description("Became Independent Contractor")]
        ACTIVATE_IC,
        /// <summary>
        /// Applies to Inactive status only
        /// </summary>
        [Description("Disabled")]
        DISABILITY,
        /// <summary>
        /// Applies to Inactive status only
        /// </summary>
        INACTIVATE,
        /// <summary>
        /// Applies to Inactive status only
        /// </summary>
        [Description("Jury Duty")]
        JURY_DUTY,
        /// <summary>
        /// Applies to Inactive status only
        /// </summary>
        [Description("Adoption Leave")]
        ADOPTION_LEAVE,
        /// <summary>
        /// Applies to Inactive status only
        /// </summary>
        [Description("Education Leave")]
        EDUCATION_LEAVE,
        /// <summary>
        /// Applies to Inactive status only
        /// </summary>
        [Description("Family Leave")]
        FAMILY_LEAVE,
        /// <summary>
        /// Applies to Inactive status only
        /// </summary>
        [Description("Maternity Leave")]
        MATERNITY_LEAVE,
        /// <summary>
        /// Applies to Inactive status only
        /// </summary>
        [Description("Medical Leave")]
        MEDICAL_LEAVE,
        /// <summary>
        /// Applies to Inactive status only
        /// </summary>
        [Description("Military Leave")]
        MILITARY_LEAVE,
        /// <summary>
        /// Applies to Inactive status only
        /// </summary>
        [Description("Paternity Leave")]
        PATERNITY_LEAVE,
        /// <summary>
        /// Applies to Inactive status only
        /// </summary>
        [Description("Seasonal Employment")]
        SEASONAL_EMPLOYMENT,
        /// <summary>
        /// Applies to Inactive status only
        /// </summary>
        [Description("Contract on Hold")]
        CONTRACT_ON_HOLD,
        /// <summary>
        /// Applies to Inactive status only
        /// </summary>
        [Description("Student on Break,")]
        STUDENT_ON_BREAK,
        /// <summary>
        /// Applies to Inactive status only
        /// </summary>
        [Description("Work is Slow")]
        WORK_IS_SLOW,
        /// <summary>
        /// Applies to Inactive status only
        /// </summary>
        /// <remarks>From https://developer.paychex.com/api-documentation-and-exploration/api-references/workers
        /// - At this time the API does not support the custom reasons that can be configured for a company
        /// </remarks>
        [Description("Unknown")]
        CUSTOM_UNKNOWN,
        /// <summary>
        /// Applies to Terminated status only
        /// </summary>
        [Description("Terminated")]
        TERMINATION,
        /// <summary>
        /// Applies to Terminated status only
        /// </summary>
        [Description("Discharged")]
        DISCHARGED,
        /// <summary>
        /// Applies to Terminated status only
        /// </summary>
        [Description("Resigned")]
        RESIGNED,
        /// <summary>
        /// Applies to Terminated status only
        /// </summary>
        [Description("Retired")]
        RETIRED,
        /// <summary>
        /// Applies to Terminated status only
        /// </summary>
        [Description("Deceased")]
        DECEASED,
        /// <summary>
        /// Applies to Terminated status only
        /// </summary>
        PEO_SERVICES_CANCELLED,
        /// <summary>
        /// Applies to Terminated status only
        /// </summary>
        [Description("Contract Terminated")]
        TERMINATE_CONTRACT,
        /// <summary>
        /// Applies to Transferred status only
        /// </summary>
        [Description("Employee Transferred")]
        EMPLOYEE_TRANSFER,
        /// <summary>
        /// Applies to Pending Employment status only
        /// </summary>
        [Description("Pending Rehire")]
        PENDING_REHIRE,
        /// <summary>
        /// Applies to Pending Employment status only
        /// </summary>
        [Description("Pending Contract")]
        PENDING_CONTRACT,
        /// <summary>
        /// Applies to Pending Employment and In Progress employment statuses only
        /// </summary>
        [Description("Pending Hire")]
        PENDING_HIRE,
        /// <summary>
        /// Applies to In Progress Employment status only
        /// </summary>
        [Description("Pending")]
        PENDING_SYNC
    }
}