using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Paychex.Api.Api.Interfaces;
using Paychex.Api.Models;
using Paychex.Api.Models.Common;
using Paychex.Api.Models.Companies;
using Paychex.Api.Models.Payroll;
using Paychex.Api.Models.Workers;
using RestSharp;
using static Paychex.Api.Api.Constants;
using CompanyLaborAssignment = Paychex.Api.Models.Common.LaborAssignment;
using WorkerCommunication = Paychex.Api.Models.Workers.Communication;

namespace Paychex.Api.Api
{
    /// <summary>
    /// Implementation of Paychex API client.
    /// </summary>
    public class ApiClient : IPaychexApiClient
    {
        private readonly ApiClientInternals _internalClient;

        /// <summary>
        /// Constructor for dependency injection.
        /// </summary>
        /// <param name="configInfo">Configuration information - api key and password, timeout info</param>
        /// <param name="clientFactory">Factory for creating RestSharp classes - used for unit testing</param>
        /// <param name="dataCache">Method for caching api requests</param>
        public ApiClient(IPaychexConfiguration configInfo,
                         IRestClientFactory clientFactory,
                         IPaychexDataCache dataCache = null
        ) => _internalClient = new ApiClientInternals(
            configInfo,
            clientFactory,
            dataCache
        );

        private Task<ApiResponse<T>> CallServerApiAsync<T>(
            string resource,
            Method method = Method.Get,
            IEnumerable<Parameter> parameters = null,
            [CallerMemberName] string caller = null
        ) => _internalClient.CallServerApiAsync<T>(resource, method, parameters, caller);

        /// <inheritdoc />
        public Task<ApiResponse<Company>> GetCompaniesAsync() => CallServerApiAsync<Company>("/companies");

        /// <inheritdoc />
        public Task<ApiResponse<Company>> GetCompanyByIdAsync(string id) =>

            CallServerApiAsync<Company>(
                $"/companies/{{{CompanyId}}}",
                parameters: new[]
                {
                    Parameter.CreateParameter(CompanyId, id, ParameterType.UrlSegment)
                }
            );

        /// <inheritdoc />
        public Task<ApiResponse<Company>> GetCompanyByDisplayIdAsync(string id) =>

            string.IsNullOrWhiteSpace(id) ? Task.FromResult<ApiResponse<Company>>(null) :
                CallServerApiAsync<Company>(
                    "/companies",
                    parameters: new[]
                    {
                        Parameter.CreateParameter(DisplayId, id, ParameterType.GetOrPost)
                    }
                );

        /// <inheritdoc />
        public Task<ApiResponse<Worker>> SearchWorkersAsync(
            WorkerSearchCriteria searchCriteria,
            Pagination pagination = null) =>

            CallServerApiAsync<Worker>(
                $"/companies/{{{CompanyId}}}/workers",
                parameters: searchCriteria.ApplyPaging(pagination)
                                          .CreateParameters()
            );

        /// <inheritdoc />
        public Task<ApiResponse<Worker>> GetWorkerByIdAsync(string workerId) =>

            CallServerApiAsync<Worker>(
                $"/workers/{{{WorkerId}}}",
                parameters: new[]
                {
                    new Parameter(WorkerId, workerId, ParameterType.UrlSegment),
                    new Parameter(
                        "Accept",
                        "application/vnd.paychex.worker_communications.v1+json",
                        ParameterType.HttpHeader
                    )
                }
            );

        /// <inheritdoc />
        public Task<ApiResponse<PayPeriod>> GetPayPeriodByIdAsync(string companyId, string payPeriodId) =>

            CallServerApiAsync<PayPeriod>(
                $"/companies/{{{CompanyId}}}/payperiods/{{{PayPeriodId}}}",
                parameters: new[]
                {
                    new Parameter(CompanyId, companyId, ParameterType.UrlSegment),
                    new Parameter(PayPeriodId, payPeriodId, ParameterType.UrlSegment)
                }
            );

        /// <inheritdoc />
        public Task<ApiResponse<PayPeriod>> SearchPayPeriodsAsync(
            string companyId,
            string status = "COMPLETED",
            DateTime? fromDate = null,
            DateTime? toDate = null) =>

            CallServerApiAsync<PayPeriod>(
                $"/companies/{{{CompanyId}}}/payperiods",
                parameters: new[]
                {
                    new Parameter(CompanyId, companyId, ParameterType.UrlSegment),
                    new OptionalParameter(
                        "status",
                        () => status,
                        ParameterType.GetOrPost,
                        () => !string.IsNullOrEmpty(status)
                    ),
                    new OptionalParameter(
                        "from",
                        () => DateTime.SpecifyKind(fromDate.GetValueOrDefault(), DateTimeKind.Utc)
                                                  .ToJsonDate(),
                        ParameterType.GetOrPost,
                        () => fromDate.HasValue && toDate.HasValue
                    ),
                    new OptionalParameter(
                        "to",
                        () => DateTime.SpecifyKind(toDate.GetValueOrDefault(), DateTimeKind.Utc)
                                                  .ToJsonDate(),
                        ParameterType.GetOrPost,
                        () => toDate.HasValue && fromDate.HasValue
                    )
                }
            );

        /// <inheritdoc />
        public Task<ApiResponse<Check>> GetWorkerChecksAsync(string workerId, string payPeriodId) =>

            CallServerApiAsync<Check>(
                $"/workers/{{{WorkerId}}}/checks",
                parameters: new[]
                {
                    new Parameter(WorkerId, workerId, ParameterType.UrlSegment),
                    new Parameter(PayPeriodId, payPeriodId, ParameterType.GetOrPost)
                }
            );

        /// <inheritdoc />
        public Task<ApiResponse<Check>> GetCompanyChecksAsync(
            string companyId,
            string payPeriodId,
            Pagination pagingSettings = null) =>

            CallServerApiAsync<Check>(
                $"/companies/{{{CompanyId}}}/checks",
                parameters: (pagingSettings?.AddPageParameters()
                                ?? new Parameter[]
                                    { }).Concat(
                    new[]
                    {
                        new Parameter(CompanyId, companyId, ParameterType.UrlSegment),
                        new Parameter(PayPeriodId, payPeriodId, ParameterType.GetOrPost),
                        new Parameter(
                            "Accept",
                            "application/vnd.paychex.payroll.processedchecks.v1+json",
                            ParameterType.HttpHeader)
                    }
                )
            );

        /// <inheritdoc />
        public Task<ApiResponse<Check>> GetCheckByIdAsync(string paycheckId, string workerId, string payPeriodId) =>

            CallServerApiAsync<Check>(
                $"/checks/{{{PayCheckId}}}",
                parameters: new[]
                {
                    new Parameter(PayCheckId, paycheckId, ParameterType.UrlSegment),
                    new Parameter(WorkerId, workerId, ParameterType.GetOrPost),
                    new Parameter(PayPeriodId, payPeriodId, ParameterType.GetOrPost)
                }
            );

        /// <inheritdoc />
        public Task<ApiResponse<PayrollComponent>> GetPayrollComponentsAsync(
            string companyId,
            string effectOnPay = null) =>

            string.IsNullOrWhiteSpace(companyId) ? Task.FromResult<ApiResponse<PayrollComponent>>(null) :
                CallServerApiAsync<PayrollComponent>(
                    $"/companies/{{{CompanyId}}}/paycomponents",
                    parameters: new[]
                    {
                        new Parameter(CompanyId, companyId, ParameterType.UrlSegment),
                        new OptionalParameter(
                            "effectOnPay",
                            () => effectOnPay,
                            ParameterType.GetOrPost,
                            () => !string.IsNullOrEmpty(effectOnPay)
                        )
                    }
                );

        /// <inheritdoc />
        public Task<ApiResponse<PayFrequencies>> GetPayFrequenciesAsync(string companyId) =>

            CallServerApiAsync<PayFrequencies>(
                $"/companies/{{{CompanyId}}}/payfrequencies",
                parameters: new[]
                {
                    new Parameter(CompanyId, companyId, ParameterType.UrlSegment)
                }
            );

        /// <inheritdoc />
        public Task<ApiResponse<CompanyLaborAssignment>> GetLaborAssignmentsAsync(string companyId) =>

            CallServerApiAsync<CompanyLaborAssignment>(
                $"/companies/{{{CompanyId}}}/laborassignments",
                parameters: new[]
                {
                    new Parameter(CompanyId, companyId, ParameterType.UrlSegment)
                }
            );

        /// <inheritdoc />
        public Task<ApiResponse<JobTitle>> GetJobTitlesAsync(string companyId) =>

            CallServerApiAsync<JobTitle>(
                $"/companies/{{{CompanyId}}}/jobtitles",
                parameters: new[]
                {
                    new Parameter(CompanyId, companyId, ParameterType.UrlSegment)
                }
            );

        /// <inheritdoc />
        public Task<ApiResponse<Job>> GetJobsAsync(string companyId) =>

            CallServerApiAsync<Job>(
                $"/companies/{{{CompanyId}}}/jobs",
                parameters: new[]
                {
                    new Parameter(CompanyId, companyId, ParameterType.UrlSegment)
                }
            );

        /// <inheritdoc />
        public async Task<ApiResponse<WorkerCommunication>> GetWorkerDemographicsAsync(string workerId)
        {
            var result = await CallServerApiAsync<WorkerCommunication>(
                $"/workers/{{{WorkerId}}}/communications",
                parameters: new[]
                {
                    new Parameter(WorkerId, workerId, ParameterType.UrlSegment)
                }
            );
            foreach (var x in result.content)
                x.WorkerId = workerId;
            return result;
        }

        /// <inheritdoc />
        public Task<ApiResponse<Organization>> GetOrganizationsAsync(string companyId) =>

            CallServerApiAsync<Organization>(
                $"/companies/{{{CompanyId}}}/organizations",
                parameters: new[]
                {
                    new Parameter(CompanyId, companyId, ParameterType.UrlSegment)
                }
            );

        /// <inheritdoc />
        public Task<ApiResponse<JobSegment>> GetJobSegmentsAsync(string companyId) =>

            CallServerApiAsync<JobSegment>(
                $"/companies/{{{CompanyId}}}/jobsegments",
                parameters: new[]
                {
                    new Parameter(CompanyId, companyId, ParameterType.UrlSegment)
                }
            );
    }
}