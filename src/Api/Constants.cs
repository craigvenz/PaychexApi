namespace Paychex.Api.Api
{
    // HTTP response headers information - https://developer.paychex.com/api-documentation-and-exploration/documentation/documentation/headers
    internal static class Constants
    {
        internal const string CompanyId = "companyId";
        internal const string PayCheckId = "paycheckId";
        internal const string PayPeriodId = "payperiodid";
        internal const string WorkerId = "workerId";
        internal const string DisplayId = "displayid";
        internal const string TransactionId = "X-payx-txid";
        internal const string ClientCorrelationId = "X-payx-client-correlationId";
        internal const string PaginationOffset = "offset";
        internal const string PaginationRecordLimit = "limit";
        internal const string PaginationId = "ETag";
    }
}
