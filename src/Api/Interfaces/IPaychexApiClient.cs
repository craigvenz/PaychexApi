// LcpMvc.Lcp.Paychex.IPaychexApiClient.cs
// Craig Venz - 09/17/2019 - 12:30 PM

using System;
using System.Threading.Tasks;
using Paychex.Api.Models;
using Paychex.Api.Models.Common;
using Paychex.Api.Models.Companies;
using Paychex.Api.Models.Payroll;
using Paychex.Api.Models.Workers;
using Communication = Paychex.Api.Models.Workers.Communication;
using LaborAssignment = Paychex.Api.Models.Common.LaborAssignment;

namespace Paychex.Api.Api.Interfaces {
    /// <summary>
    /// 
    /// </summary>
    public interface IPaychexApiClient {
        /// <summary>
        /// Lists companies available to the API
        /// </summary>
        /// <returns>List of companies.</returns>
        Task<ApiResponse<Company>> GetCompaniesAsync();

        /// <summary>
        /// Get company information by a specific paychex company id
        /// </summary>
        /// <param name="id">company id</param>
        /// <returns><see cref="Company">Company object</see> for that company</returns>
        Task<ApiResponse<Company>> GetCompanyByIdAsync(string id);

        /// <summary>
        /// Get company information by Display Id - this data is found in the Paychex web application.
        /// </summary>
        /// <param name="id">Display Id</param>
        /// <returns><see cref="Company">Company</see> object, or null if <paramref name="id"/> is null or empty.</returns>
        Task<ApiResponse<Company>> GetCompanyByDisplayIdAsync(string id);

        /// <summary>
        /// Search for workers associated with a company by parameters
        /// </summary>
        /// <param name="searchCriteria">
        /// - WorkerSearch(string companyId, string employeeId, PagingInfo = null)
        /// - WorkerSearch(string companyId, string firstName = null, string lastName = null, string ssnLastFour = null, PagingInfo = null)
        /// - WorkerSearch(string companyId, DateTime startDate, DateTime endDate, PagingInfo = null)
        /// </param>
        /// <param name="pagination"></param>
        /// <returns><see cref="Worker">Worker object</see></returns>
        Task<ApiResponse<Worker>> SearchWorkersAsync(WorkerSearchCriteria searchCriteria, Pagination pagination = null);

        /// <summary>
        /// Retrieve a worker by their worker Id
        /// </summary>
        /// <param name="workerId"></param>
        /// <returns><see cref="Worker">Worker object</see></returns>
        Task<ApiResponse<Worker>> GetWorkerByIdAsync(string workerId);

        /// <summary>
        /// Get information about a specific pay period
        /// </summary>
        /// <param name="companyId">Company id</param>
        /// <param name="payPeriodId">Pay period id</param>
        /// <returns><see cref="PayPeriod">Pay Period</see> object</returns>
        Task<ApiResponse<PayPeriod>> GetPayPeriodByIdAsync(string companyId, string payPeriodId);

        /// <summary>
        /// Get a list of pay periods by search criteria
        /// </summary>
        /// <param name="companyId">Company id</param>
        /// <param name="fromDate">Date to begin search</param>
        /// <param name="toDate">Date to end search</param>
        /// <param name="status">Status to search for</param>
        /// <returns>List of <see cref="PayPeriod">pay periods</see></returns>
        /// <remarks>
        /// If no from date and to date is specified, all pay periods <b>that are unprocessed</b> are returned.
        /// If passing a date, both fromDate and toDate are required. This date applies to all date fields in the pay period:
        /// start date, end date, submit by date, check date
        /// </remarks>
        Task<ApiResponse<PayPeriod>> SearchPayPeriodsAsync(string companyId,
                                                           string status = null,
                                                           DateTime? fromDate = null,
                                                           DateTime? toDate = null);

        /// <summary>
        /// Get paychecks for a specific worker in a pay period
        /// </summary>
        /// <param name="workerId">worker id</param>
        /// <param name="payPeriodId">pay period id</param>
        /// <returns>List of <see cref="Check">check</see> objects</returns>
        Task<ApiResponse<Check>> GetWorkerChecksAsync(string workerId, string payPeriodId);

        /// <summary>
        /// Get all checks in a pay period for a company
        /// </summary>
        /// <param name="companyId">company id</param>
        /// <param name="payPeriodId">pay period id</param>
        /// <param name="pagingSettings">optional info to facilitate paging. <see cref="Pagination"/></param>
        /// <returns>List of <see cref="Check">check</see> objects</returns>
        Task<ApiResponse<Check>> GetCompanyChecksAsync(string companyId, string payPeriodId, Pagination pagingSettings = null);

        /// <summary>
        /// Get a check by it's Id.
        /// </summary>
        /// <param name="paycheckId">paycheck id</param>
        /// <param name="workerId">worker who owns the check</param>
        /// <param name="payPeriodId">pay period in which the check falls</param>
        /// <returns><see cref="Check">check</see> object</returns>
        Task<ApiResponse<Check>> GetCheckByIdAsync(string paycheckId, string workerId, string payPeriodId);

        /// <summary>
        /// Get the list of payroll components a company uses in its checks.
        /// </summary>
        /// <param name="companyId">company id</param>
        /// <param name="effectOnPay">effect on pay. Valid values: ADDITION_WITH_IN_OUT, ADDITION, REDUCTION</param>
        /// <returns>List of <see cref="PayrollComponent">payroll component</see> objects, 
        /// or null if <paramref name="companyId"/> is null or empty.</returns>
        Task<ApiResponse<PayrollComponent>> GetPayrollComponentsAsync(
            string companyId,
            string effectOnPay = null);

        /// <summary>
        /// Get list of company pay frequencies
        /// </summary>
        /// <param name="companyId">company id</param>
        /// <returns>List of <see cref="PayFrequencies">Pay Frequencies</see></returns>
        Task<ApiResponse<PayFrequencies>> GetPayFrequenciesAsync(string companyId);

        /// <summary>
        /// Get list of labor assignments used by a company
        /// </summary>
        /// <param name="companyId">company id</param>
        /// <returns>List of <see cref="LaborAssignment">labor assignments</see></returns>
        Task<ApiResponse<LaborAssignment>> GetLaborAssignmentsAsync(string companyId);

        /// <summary>
        /// Get list of job titles used by a company
        /// </summary>
        /// <param name="companyId">company id</param>
        /// <returns>List of <see cref="JobTitle">job titles</see></returns>
        Task<ApiResponse<JobTitle>> GetJobTitlesAsync(string companyId);

        /// <summary>
        /// Get list of jobs used at a company
        /// </summary>
        /// <param name="companyId">company id</param>
        /// <returns>List of <see cref="Job">jobs</see></returns>
        Task<ApiResponse<Job>> GetJobsAsync(string companyId);

        /// <summary>
        /// Get list of contact methods for a worker
        /// </summary>
        /// <param name="workerId">worker id</param>
        /// <returns>list of '<see cref="Communication">Communications</see>' objects</returns>
        Task<ApiResponse<Communication>> GetWorkerDemographicsAsync(string workerId);

        /// <summary>
        /// Get list of organizations in a company
        /// </summary>
        /// <param name="companyId">company id</param>
        /// <returns>List of <see cref="Organization">organizations</see></returns>
        Task<ApiResponse<Organization>> GetOrganizationsAsync(string companyId);

        /// <summary>
        /// Get list of job segments in a company
        /// </summary>
        /// <param name="companyId">company id</param>
        /// <returns>List of <see cref="JobSegment">Job Segments</see></returns>
        Task<ApiResponse<JobSegment>> GetJobSegmentsAsync(string companyId);
    }
}