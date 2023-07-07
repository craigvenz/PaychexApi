using System;
using System.Collections.Generic;
using Paychex.Api.Models.Common;
using RestSharp;
using static Paychex.Api.Api.Extensions;

namespace Paychex.Api.Api
{
    /// <summary>
    ///
    /// </summary>
    public class WorkerSearchCriteria : Pagination
    {
        private const string PaychexWorkerGivenName = "givenName";
        private const string PaychexWorkerFamilyName = "familyName";
        private const string PaychexWorkerLastFour = "legalLastFour";
        private const string PaychexEmployeeId = "employeeId";
        private const string PaychexFromDate = "from";
        private const string PaychexToDate = "to";

        /// <summary>
        ///
        /// </summary>
        public string CompanyId { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string LastFour { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string EmployeeId { get; set; }

        /// <summary>
        ///
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        ///
        /// </summary>
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// Creates the necessary parameters for getting Workers by Company.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Parameter> CreateParameters()
        {
            // parameter rules for the WorkerSearch api call
            // implements parameters analogous to these overloads:
            // - WorkerSearch(string companyId, string employeeId, PagingInfo = null)
            // - WorkerSearch(string companyId, string firstName = null, string lastName = null, string ssnLastFour = null, PagingInfo = null)
            // - WorkerSearch(string companyId, DateTime startDate, DateTime endDate, PagingInfo = null)

            var l = new List<Parameter>
            {
                MakeParam(Constants.CompanyId, CompanyId, ParameterType.UrlSegment),
                // makes api return worker communications - https://developer.paychex.com/documentation#operation/getCompanyWorkers
                // information on Profiles - https://developer.paychex.com/resources/vendor-media-type
                MakeParam(
                    "Accept",
                    "application/vnd.paychex.workers_communications.v1+json",
                    ParameterType.HttpHeader
                )
            };
            l.AddRange(this.AddPageParameters());

            // If we supplied an employee id we can't add anything else
            if (!string.IsNullOrEmpty(EmployeeId))
            {
                l.Add(MakeParam(PaychexEmployeeId, EmployeeId));
                return l;
            }

            // If we supplied names to search for, that's all we can use and not anything else
            bool anyAdded;
            (l, anyAdded) = AddNameParameters(l);
            if (anyAdded)
                return l;

            // only dates can be allowed at this point for doing a date range search
            l.AddRange(AddDateParameters());
            return l;
        }

        private IEnumerable<Parameter> AddDateParameters()
        {
            if (FromDate.HasValue)
                yield return MakeParam(PaychexFromDate, FromDate.Value.ToJsonDate());

            if (ToDate.HasValue)
                yield return MakeParam(PaychexToDate, ToDate.Value.ToJsonDate());
        }

        private (List<Parameter> returnValue, bool success) AddNameParameters(List<Parameter> l)
        {
            var c = l.Count;
            if (!string.IsNullOrEmpty(FirstName))
                l.Add(MakeParam(PaychexWorkerGivenName, FirstName));

            if (!string.IsNullOrEmpty(LastName))
                l.Add(MakeParam(PaychexWorkerFamilyName, LastName));

            if (!string.IsNullOrEmpty(LastFour))
                l.Add(MakeParam(PaychexWorkerLastFour, LastName));

            return (l, l.Count > c);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pagination"></param>
        /// <returns></returns>
        public WorkerSearchCriteria ApplyPaging(Pagination pagination)
        {
            if (pagination != null)
            {
                Offset = pagination.Offset;
                Limit = pagination.Limit;
                ETag = pagination.ETag;
            }
            return this;
        }
    }
}