using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Paychex.Api.Api.Interfaces;
using Paychex.Api.Models;
using RestSharp;

namespace Paychex.Api.Api
{
    internal sealed class ApiClientInternals
    {
        private readonly IPaychexConfiguration _authInfo;
        private readonly IRestClientFactory _restClientFactory;
        private readonly IPaychexDataCache _dataCache;

        internal ApiClientInternals(
            IPaychexConfiguration authInfo,
            IRestClientFactory restClientFactory,
            IPaychexDataCache dataCache = null
        )
        {
            _authInfo = authInfo;
            _restClientFactory = restClientFactory;
            _dataCache = dataCache;
        }

        internal async Task<ApiResponse<T>> CallServerApiAsync<T>(string resource,
                                                                  Method method = Method.Get,
                                                                  IEnumerable<Parameter> parameters = null,
                                                                  string caller = null)
        {
            Uri url;

            var timer = new Stopwatch();

            var paramList = parameters?.ToList() ?? new List<Parameter>();

            void DebugInfo(string message) => Trace.TraceInformation(
$@"({timer.ElapsedMilliseconds}ms, tid {Thread.CurrentThread.ManagedThreadId}) Api call {caller ?? GetType().FullName}
Url: ""{url.OriginalString}""
Parameters: {paramList.ListOrNull(predicate: x => x.Type != ParameterType.UrlSegment)}
{message}"
            );

            var client = CreateClient();

            var request = CreateRequest(resource, method, paramList);

            url = client.BuildUri(request);

            var r = HttpUtility.UrlEncode(url.PathAndQuery);
            try
            {
                var cached = _dataCache.Get<ApiResponse<T>>(r);
                if (cached != null)
                {
                    DebugInfo("Returned cached response (No authentication needed)\n");
                    return cached;
                }
            }
            catch (Exception)
            {
                Trace.TraceError("Failed deserializing cache. Check cache contents.");
                throw;
            }

            Trace.TraceInformation(
                $"correlation id before send is: {request.Parameters?.FirstOrDefault(x => x.Name.Equals(Constants.ClientCorrelationId, StringComparison.OrdinalIgnoreCase))}"
            );

            timer.Start();

            var (result, response) = await ExecuteRequest<T>(client, request)
                .ConfigureAwait(false);

            timer.Stop();

            DebugInfo($"Returned status {response.ResponseStatus}, http status code {response.StatusCode}\n");

            if (response.IsSuccessful && result?.success == true)
            {
                _dataCache.Set(r, result);
                return result;
            }

            if (result?.errors != null)
                throw new PaychexApiException(response.ResponseUri ?? url, result?.errors);

            TryHandleInternalServerError(response);

            throw new PaychexException($"Error while importing: Server returned {response.StatusCode}, {response.StatusDescription}");
        }

        private static void TryHandleInternalServerError(RestResponse response)
        {
            if (string.IsNullOrWhiteSpace(response.Content))
                return;

            var error = JsonConvert.DeserializeObject<InternalServerError>(response.Content);
            if (error != null)
                throw new PaychexApiException(response.ResponseUri, $"Error while importing: {error.Error}");
        }

        private IRestClient CreateClient()
        {
            var client = _restClientFactory.CreateClient();
            client.Options.BaseUrl = new Uri(_authInfo.UrlEndpoint);
            client.Options.Authenticator = _restClientFactory.CreateAuthenticator();
            client.Options.MaxTimeout = (int)_authInfo.Timeout.TotalMilliseconds;
            return client;
        }

        private RestRequest CreateRequest(
            string resource,
            Method method,
            IEnumerable<Parameter> parameters)
        {
            var correlationId = Guid.NewGuid()
                                    .ToString("n");

            var request = _restClientFactory.CreateRequest();
            request.Method = method;
            request.Resource = resource;
            request.JsonSerializer = new JsonNetSerializer();
            request.AddParameter(Constants.ClientCorrelationId, correlationId, ParameterType.HttpHeader);
            foreach (var p in parameters ?? new Parameter[] { })
            {
                if (p is OptionalParameter op)
                {
                    request.AddOptionalParameter(op.Name, op.ValueFunc, op.Condition);
                }
                else
                {
                    request.AddParameter(p);
                }
            }
            if (!request.Parameters?.Any(x => x.Type == ParameterType.HttpHeader && x.Name == "Accept") == true)
                request.AddHeader("Accept", "application/json");

            return request;
        }

        private async Task<(ApiResponse<T> result, RestResponse response)> ExecuteRequest<T>(
            IRestClient client,
            RestRequest request
        )
        {
            try
            {
                var task = client.ExecuteAsync(request);

                var (response, result) = await AwaitResponseWithTimeout<T>(task);

                SetVariablesFromHeaders(result, response);

                return (result, response);
            }
            catch (TimeoutException tex)
            {
                throw new PaychexException("Timeout", tex);
            }
        }

        private async Task<(RestResponse response, ApiResponse<T> result)> 
            AwaitResponseWithTimeout<T>(
            Task<RestResponse> task
        ) 
        {
            try
            {
                var t = task.Wait(_authInfo.Timeout);
                if (!t)
                    throw new TimeoutException("Timeout");
            }
            catch (AggregateException ex)
            {
                throw new PaychexApiException(ex.InnerExceptions);
            }

            var response = await task;

            if (response is { IsSuccessful: false, ErrorException: { } })
            {
                return (response, new ApiResponse<T>(new ApiError(response.ErrorException)));
            }
            try
            {
                var result = new JsonNetSerializer().Deserialize<ApiResponse<T>>(response);
                return (response, result);
            }
            catch (Exception ex)
            {
                return (response, new ApiResponse<T>(new ApiError(ex)));
            }
        }

        private static void SetVariablesFromHeaders<T>(ApiResponse<T> result,
                                                       RestResponseBase response)
        {
            try
            {
                result.correlationId = response.Headers
                                               .FirstOrDefault(h => h.Name.Equals(Constants.ClientCorrelationId, StringComparison.OrdinalIgnoreCase))?
                                               .Value?
                                               .ToString();
                var txId = response.Headers
                                   .FirstOrDefault(h => h.Name.Equals(Constants.TransactionId, StringComparison.OrdinalIgnoreCase))?
                                   .Value?
                                   .ToString();

                result.paychexTransactionId = txId;
                foreach (var item in result.errors ?? new List<ApiError>())
                {
                    item.transactionId = txId;
                }

                if (result.metadata?.Pagination != null)
                {
                    result.metadata.Pagination.ETag =
                        response.Headers
                                .FirstOrDefault(h => h.Name.Equals(Constants.PaginationId, StringComparison.OrdinalIgnoreCase))?
                                .Value?
                                .ToString();
                }
            }
            finally
            {
                /*
                Trace.Indent();
                Debug.WriteLine(response.Headers.ListOrNull(",\n\t"));
                Trace.Unindent();
                */
                Trace.TraceInformation(
$@"Debug Ids
client correlation id: {result.correlationId}
paychex transaction id: {result.paychexTransactionId}
"
                );
                if (result.metadata?.Pagination != null)
                    Trace.TraceInformation($"etag: {result.metadata.Pagination.ETag}");
            }
        }

        private sealed class JsonNetSerializer : RestSharp.Serializers.ISerializer
        {
            private readonly JsonSerializerSettings _settings;

            public JsonNetSerializer() => _settings =
                new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    MissingMemberHandling = MissingMemberHandling.Error,
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    NullValueHandling = NullValueHandling.Ignore,
                    StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                    Converters = new List<JsonConverter>
                    {
                        new TolerantEnumConverter()
                    }
                };

            public T Deserialize<T>(RestResponseBase response)
            {
                (T, Exception) TryDeserialize()
                {
                    try
                    {
                        return (JsonConvert.DeserializeObject<T>(response.Content, _settings), null);
                    }
                    catch (Exception e)
                    {
                        return (default, e);
                    }
                }

                var (result, ex) = TryDeserialize();
                if (result != null)
                    return result;

                if (!(ex is JsonSerializationException) || !ex.Message.StartsWith("Could not find member"))
                {
                    Trace.TraceError(
                        "Some other issue happened while deserializing json. Contents: {0}",
                        response?.Content
                    );
                    throw ex;
                }

                _settings.MissingMemberHandling = MissingMemberHandling.Ignore;
                Trace.TraceWarning(
                    $"Alert: A missing member on an object was found - please check and update the paychex models.\n{ex}"
                );

                (result, ex) = TryDeserialize();
                if (result != null)
                    return result;

                throw ex;
            }

            public ContentType ContentType { get; set; } = ContentType.Json;

            public string Serialize(object obj) => JsonConvert.SerializeObject(obj, _settings);
        }
    }
}