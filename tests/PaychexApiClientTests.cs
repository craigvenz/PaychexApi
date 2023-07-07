using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Idioms;
using FluentAssertions;
using Lcp.Paychex.Api;
using Lcp.Paychex.Api.Interfaces;
using Lcp.Paychex.Models;
using Lcp.Paychex.Models.Authentication;
using Lcp.Paychex.Models.Common;
using Lcp.Paychex.Models.Companies;
using Lcp.Paychex.Models.Payroll;
using Lcp.Paychex.Models.Workers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using RestSharp;

namespace Lcp.Services.Tests.Paychex
{
    [TestClass]
    public class PaychexApiClientTests : TestBase
    {
        private readonly DummyConfig _dummyConfig = new DummyConfig();
        private readonly DummyCache _tokenCache = new DummyCache();

        private static (Mock<IRestClientFactory> factory, Mock<IRestClient> client, Mock<IRestRequest> request)
            CreateRestSharpMocks([CallerMemberName] string caller = "")
        {
            var mockRestClient = CreateTestMock<IRestClient>(caller);
            var mockRestRequest = CreateTestMock<IRestRequest>(caller);
            var clientFactory = CreateTestMock<IRestClientFactory>(caller);
            clientFactory.Setup(c => c.CreateClient())
                         .Returns(mockRestClient.Object);
            clientFactory.Setup(c => c.CreateRequest())
                         .Returns(mockRestRequest.Object);
            return (clientFactory, mockRestClient, mockRestRequest);
        }

        private static (string txId, string cId) SetupResponse(Mock<IRestResponse> response,
                                                                Action<Mock<IRestResponse>> actions,
                                                                string txId = null, string corrId = null)
        {
            if (string.IsNullOrEmpty(txId))
                txId = Guid.NewGuid().ToString("n");
            if (string.IsNullOrEmpty(corrId))
                corrId = Guid.NewGuid().ToString("n");

            actions(response);

            response.SetupGet(r => r.Headers)
                    .Returns(
                        new List<Parameter>
                        {
                            new Parameter(Constants.TransactionId, txId, ParameterType.HttpHeader),
                            new Parameter(Constants.ClientCorrelationId, corrId, ParameterType.HttpHeader)
                        }
                    );

            return (txId, corrId);
        }

        private static (string txId, string cId) SetSuccessfulResponse(
            Mock<IRestResponse> response,
            string txId = null,
            string corrId = null)
        {
            void SetSuccess(Mock<IRestResponse> resp)
            {
                resp.SetupGet(r => r.IsSuccessful)
                    .Returns(true);
                resp.SetupGet(r => r.ResponseStatus)
                    .Returns(ResponseStatus.Completed);
                resp.SetupGet(r => r.StatusCode)
                    .Returns(HttpStatusCode.OK);
            }
            return SetupResponse(response, SetSuccess, txId, corrId);
        }

        /*
        [TestInitialize]
        public void Initialize()
        {
        }
        */

        [TestMethod]
        public async Task ApiCallWithPaginationReturnsETag()
        {
            const string testCompanyId = "00Z6LYF217B6F42AQR0E";

            var (factory, client, _) = CreateRestSharpMocks();

            var apiClient = new ApiClient(
                _dummyConfig,
                factory.Object,
                _tokenCache
            );

            var response = CreateTestMock<IRestResponse>();

            client.Setup(c => c.BuildUri(It.IsAny<IRestRequest>()))
                  .Returns(new Uri(_dummyConfig.UrlEndpoint));

            var (tid, cid) = SetSuccessfulResponse(response);

            var etagInitial = Guid.NewGuid()
                                  .ToString("n");

            response.SetupGet(r => r.Headers)
                    .Returns(
                        new List<Parameter>
                        {
                            new Parameter(Constants.TransactionId, tid, ParameterType.HttpHeader),
                            new Parameter(Constants.ClientCorrelationId, cid, ParameterType.HttpHeader),
                            new Parameter(Constants.PaginationId, etagInitial, ParameterType.HttpHeader)
                        }
                    );

            client.Setup(c => c.ExecuteTaskAsync(It.IsAny<IRestRequest>()))
                  .ReturnsAsync(response.Object);

            response.SetupGet(r => r.Content)
                    .Returns(
                        JsonConvert.SerializeObject(
                            new ApiResponse<Worker>
                            {
                                metadata = new Metadata
                                {
                                    ContentItemCount = 10,
                                    Pagination = new Pagination
                                    {
                                        ETag = etagInitial,
                                        Offset = 0,
                                        Limit = 5,
                                        ItemCount = 5
                                    }
                                },
                                content = PaychexDataGenerator.GenerateWorker(null, null)
                                                              .Generate(5)
                                                              .ToList()
                            }
                        )
                    );

            var result = await apiClient.SearchWorkersAsync(
                new WorkerSearchCriteria
                {
                    CompanyId = testCompanyId
                }
            );
            result.content.Should().HaveCount(5);
            result.metadata.Pagination.ETag.Should()
                  .NotBeEmpty();
        }

        [Ignore]
        [TestMethod]
        public void PaginatedCallsCorrectlyPassBackETagOnSubsequentCalls()
        {
            var (factory, client, _) = CreateRestSharpMocks();

            var apiClient = new ApiClient(
                _dummyConfig,
                factory.Object,
                _tokenCache
            );

            var response = CreateTestMock<IRestResponse<ApiResponse<Company>>>();
            client.Setup(c => c.ExecuteTaskAsync(It.IsAny<IRestRequest>()))
                  .ReturnsAsync(response.Object);
            client.Setup(c => c.BuildUri(It.IsAny<IRestRequest>()))
                  .Returns(new Uri(_dummyConfig.UrlEndpoint));

            Assert.Inconclusive("Not finished, no asserts");
        }

        [TestMethod]
        public async Task ProperlyThrowsExceptionIfResponseIsNot200()
        {
            var (factory, client, _) = CreateRestSharpMocks();

            var apiClient = new ApiClient(
                _dummyConfig,
                factory.Object,
                _tokenCache
            );

            var endPoint = new Uri(_dummyConfig.UrlEndpoint);

            var response = CreateTestMock<IRestResponse>();
            client.Setup(c => c.ExecuteTaskAsync(It.IsAny<IRestRequest>()))
                  .ReturnsAsync(response.Object);
            client.Setup(c => c.BuildUri(It.IsAny<IRestRequest>()))
                  .Returns(endPoint);

            SetupResponse(
                response,
                r =>
                {
                    r.SetupGet(o => o.IsSuccessful)
                     .Returns(false);
                    r.SetupGet(o => o.StatusCode)
                     .Returns(HttpStatusCode.InternalServerError);
                    r.SetupGet(o => o.ErrorException)
                     .Returns(new WebException("test error", WebExceptionStatus.UnknownError));
                    r.SetupGet(o => o.Content)
                     .Returns(JsonConvert.SerializeObject(new ApiResponse<Company>()));
                    r.SetupGet(o => o.ResponseUri)
                     .Returns(endPoint);
                }
            );

            Func<Task> perform = () => apiClient.GetCompaniesAsync();

            await perform.Should()
                         .ThrowAsync<PaychexApiException>();
            /*
                        response.SetupGet(o => o.ErrorException)
                                .Returns((Exception)null);

                        await perform.Should()
                                     .ThrowAsync<PaychexException>()
                                     .WithMessage("Unknown problem:*");
            */
        }

        private PaychexAuthToken CreateDummyToken(DateTime when) =>
            new PaychexAuthToken
            {
                access_token = Guid.NewGuid()
                                   .ToString("d"),
                scope = "oob",
                expires_in = 3600,
                token_type = "bearer",
                TimeAuthenticated = when
            };

        [TestMethod]
        public void TokenIsRefreshedIfCachedTokenIsNull()
        {
            var (factory, client, request) = CreateRestSharpMocks();

            var cacheMock = CreateTestMock<IPaychexTokenCache>();

            var auth = new PaychexAuthenticator(_dummyConfig, cacheMock.Object, factory.Object);

            client.Setup(c => c.BuildUri(It.IsAny<IRestRequest>()))
                  .Returns(new Uri(_dummyConfig.UrlEndpoint));
            request.Setup(r => r.AddParameter(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<ParameterType>()))
                   .Returns(request.Object);

            var successToken = CreateDummyToken(DateTime.Now);

            var response = CreateTestMock<IRestResponse<PaychexAuthToken>>();
            response.SetupGet(r => r.IsSuccessful)
                    .Returns(true);
            response.SetupGet(r => r.ResponseStatus)
                    .Returns(ResponseStatus.Completed);
            response.SetupGet(r => r.StatusCode)
                    .Returns(HttpStatusCode.OK);
            response.SetupGet(x => x.Data)
                    .Returns(successToken);

            cacheMock.Setup(c => c.Save(It.Is<PaychexAuthToken>(a => a == successToken)))
                     .Verifiable();

            client.Setup(c => c.Execute<PaychexAuthToken>(It.Is<IRestRequest>(x => x == request.Object)))
                  .Returns(response.Object);

            auth.Authenticate(client.Object, request.Object);

            cacheMock.Verify();
        }

        [TestMethod]
        public void TokenIsRefreshedIfCachedTokenIsExpired()
        {
            var (factory, client, request) = CreateRestSharpMocks();

            var cacheMock = CreateTestMock<IPaychexTokenCache>();

            var auth = new PaychexAuthenticator(_dummyConfig, cacheMock.Object, factory.Object);

            client.Setup(c => c.BuildUri(It.IsAny<IRestRequest>()))
                  .Returns(new Uri(_dummyConfig.UrlEndpoint));
            request.Setup(r => r.AddParameter(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<ParameterType>()))
                   .Returns(request.Object);

            var successToken = CreateDummyToken(DateTime.Now);
            var oldToken = CreateDummyToken(DateTime.Now.AddHours(-1));

            var response = CreateTestMock<IRestResponse<PaychexAuthToken>>();
            response.SetupGet(r => r.IsSuccessful)
                    .Returns(true);
            response.SetupGet(r => r.ResponseStatus)
                    .Returns(ResponseStatus.Completed);
            response.SetupGet(r => r.StatusCode)
                    .Returns(HttpStatusCode.OK);

            response.SetupGet(x => x.Data)
                    .Returns(successToken);

            cacheMock.Setup(c => c.Load())
                     .Returns(oldToken);
            cacheMock.Setup(c => c.Save(It.Is<PaychexAuthToken>(a => a == successToken)))
                     .Verifiable();

            client.Setup(c => c.Execute<PaychexAuthToken>(It.Is<IRestRequest>(x => x == request.Object)))
                  .Returns(response.Object);

            auth.Authenticate(client.Object, request.Object);

            cacheMock.Verify();
        }

        [Ignore]
        [TestMethod]
        public void MethodsSupportingPaginationAreCalledMultipleTimesAsAppropriate()
            => Assert.Inconclusive("Not implemented");

        [TestMethod]
        public async Task TimeoutIsHonored()
        {
            var (factory, client, request) = CreateRestSharpMocks();

            var apiClient = new ApiClient(
                _dummyConfig,
                factory.Object,
                _tokenCache
            );

            client.Setup(c => c.ExecuteTaskAsync(It.Is<IRestRequest>(m => m == request.Object)))
                  .Throws<TimeoutException>();

            client.Setup(c => c.BuildUri(It.IsAny<IRestRequest>()))
                  .Returns(new Uri(_dummyConfig.UrlEndpoint));

            Func<Task> perform = () => apiClient.GetCompaniesAsync();

            await perform.Should()
                         .ThrowAsync<PaychexException>()
                         .WithMessage("Timeout");
        }

        [TestMethod]
        public async Task RequestTimeoutThrowsTimeoutException()
        {
            var (factory, client, request) = CreateRestSharpMocks();

            var apiClient = new ApiClient(
                new DummyConfig
                {
                    Timeout = new TimeSpan(1)
                },
                factory.Object,
                _tokenCache
            );

            client.Setup(c => c.BuildUri(It.IsAny<IRestRequest>()))
                  .Returns(new Uri(_dummyConfig.UrlEndpoint));

            var delayTask = new Task<IRestResponse>(
                () =>
                {
                    Thread.Sleep(1000);
                    return CreateTestMock<IRestResponse>().Object;
                }
            );

            client.Setup(c => c.ExecuteTaskAsync(It.Is<IRestRequest>(m => m == request.Object)))
                  .Returns(delayTask);

            Func<Task> act = () => apiClient.GetCompaniesAsync();

            await act.Should()
                     .ThrowAsync<PaychexException>()
                     .WithMessage("Timeout");
        }

        [TestMethod]
        public async Task GetCompaniesAsync_MakesApiCalls()
        {
            var (factory, client, _) = CreateRestSharpMocks();

            var apiClient = new ApiClient(
                _dummyConfig,
                factory.Object,
                _tokenCache
            );

            var response = CreateTestMock<IRestResponse>();

            client.Setup(c => c.ExecuteTaskAsync(It.IsAny<IRestRequest>()))
                  .ReturnsAsync(response.Object);
            client.Setup(c => c.BuildUri(It.IsAny<IRestRequest>()))
                  .Returns(new Uri(_dummyConfig.UrlEndpoint));

            response.SetupGet(r => r.Content)
                    .Returns(JsonConvert.SerializeObject(new ApiResponse<Company>()));

            var (pxDummyId, corrId) = SetSuccessfulResponse(response);
            var result = await apiClient.GetCompaniesAsync();

            result.Should().NotBeNull();
            result.correlationId.Should()
                  .BeEquivalentTo(corrId);
            result.paychexTransactionId.Should()
                  .BeEquivalentTo(pxDummyId);
        }

        [TestMethod]
        public void GetHashCodeImplementations()
        {
            var f = new Fixture();
            Action act = () => new GetHashCodeSuccessiveAssertion(f.Build<Check>()).Verify(typeof(Check).GetMethod("GetHashCode"));
            act.Should()
               .NotThrow<GetHashCodeOverrideException>();
        }

        [TestMethod]
        public async Task GetCompanyByDisplayIdAsync_NullTest()
        {
            // Arrange
            var (factory, _, _) = CreateRestSharpMocks();

            var apiClient = new ApiClient(
                _dummyConfig,
                factory.Object,
                _tokenCache
            );

            // Act
            var result = await apiClient.GetCompanyByDisplayIdAsync(string.Empty);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task GetPayrollComponentsAsync_NullTest()
        {
            // Arrange
            var (factory, _, _) = CreateRestSharpMocks();

            var apiClient = new ApiClient(
                _dummyConfig,
                factory.Object,
                _tokenCache
            );

            // Act
            var result = await apiClient.GetPayrollComponentsAsync(string.Empty);

            // Assert
            result.Should().BeNull();
        }

        private class DummyConfig : IPaychexConfiguration
        {
            public string UrlEndpoint => "https://localhost";
            public string ApiKey => string.Empty;
            public string ClientSecret => string.Empty;
            public TimeSpan Timeout { get; set; } = new TimeSpan(0, 0, 10);
        }

        private class DummyCache : IPaychexDataCache, IPaychexTokenCache
        {
            public void Set<T>(string r, T value)
            {
            }

            public T Get<T>(string r) => default(T);

            public void Clear()
            {
            }

            public bool IgnoreCacheReads { get; set; }

            public PaychexAuthToken Load() => null;

            public void Save(PaychexAuthToken token)
            {
            }

            public void Invalidate()
            {
            }
        }
    }
}