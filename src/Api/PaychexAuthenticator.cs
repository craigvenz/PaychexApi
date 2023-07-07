using System;
using System.Threading;
using Paychex.Api.Api.Interfaces;
using Paychex.Api.Models;
using Paychex.Api.Models.Authentication;

namespace Paychex.Api.Api
{
    /// <summary>
    ///     Authenticator class for RestSharp - customized for Paychex since they don't use JWT, they use their own thing
    /// </summary>
    public class PaychexAuthenticator : IAuthenticator
    {
        private const string UsernameKey = "client_id";
        private const string PasswordKey = "client_secret";

        private static readonly SemaphoreSlim AuthorizationLock = new SemaphoreSlim(1, 1);
        private readonly IPaychexConfiguration _authInfo;
        private readonly IPaychexTokenCache _tokenCache;
        private readonly IRestClientFactory _clientFactory;
        private PaychexAuthToken _token;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="tokenCache"></param>
        /// <param name="authInfo"></param>
        /// <param name="clientFactory"></param>
        public PaychexAuthenticator(IPaychexConfiguration authInfo,
                                    IPaychexTokenCache tokenCache,
                                    IRestClientFactory clientFactory)
        {
            _authInfo = authInfo;
            _tokenCache = tokenCache;
            _clientFactory = clientFactory;
        }

        /// <inheritdoc />
        public void Authenticate(IRestClient client, IRestRequest request)
        {
            if (CheckToken())
            {
                SetAuthenticationHeader(request);
                return;
            }

            // Unsure if this is necessary but I am using a singleton client to cache the token. I thought there
            // could be a possible an issue here. Its a race condition when running multiple requests in parallel, 
            // which could result in multiple authorization calls that are unnecessary.
            AuthorizationLock.Wait(
                (int) TimeSpan.FromSeconds(30)
                              .TotalMilliseconds
            );
            try
            {
                var result = CallPaychexAuthenticationEndpoint(client);
                if (result.IsSuccessful)
                {
                    SetToken(result.Data);
                    SetAuthenticationHeader(request);
                }
                else
                {
                    var error = JsonConvert.DeserializeObject<PaychexError>(result.Content);
                    throw new PaychexAuthenticationException(error);
                }
            }
            finally
            {
                AuthorizationLock.Release();
            }
        }

        private bool CheckToken()
        {
            if (_token?.isValid == true)
                return true;
            _token = _tokenCache?.Load();
            return _token?.isValid == true;
        }

        private void SetToken(PaychexAuthToken token)
        {
            _token = token;
            _token.TimeAuthenticated = DateTime.Now;
            _tokenCache?.Save(_token);
        }

        private void SetAuthenticationHeader(IRestRequest request)
        {
            request.AddHeader("Authorization", $"{_token.token_type} {_token.access_token}");
        }

        private IRestResponse<PaychexAuthToken> CallPaychexAuthenticationEndpoint(IRestClient parent)
        {
            var internalClient = _clientFactory.CreateClient();
            internalClient.BaseUrl = parent.BaseUrl;
            internalClient.Timeout = parent.Timeout;

            var internalRequest = _clientFactory.CreateRequest();
            internalRequest.AddParameter("grant_type", "client_credentials", ParameterType.GetOrPost)
                           .AddParameter(UsernameKey, _authInfo.ApiKey, ParameterType.GetOrPost)
                           .AddParameter(PasswordKey, _authInfo.ClientSecret, ParameterType.GetOrPost);
            internalRequest.Resource = "auth/oauth/v2/token";
            internalRequest.Method = Method.POST;

            return internalClient.Execute<PaychexAuthToken>(internalRequest);
        }
    }
}
