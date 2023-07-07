using Paychex.Api.Api.Interfaces;

namespace Paychex.Api.Api
{
    /// <summary>
    /// Default implementation which returns concrete RestSharp classes. 
    /// </summary>
    public class PaychexClientFactory : Interfaces.IRestClientFactory
    {
        private readonly IPaychexConfiguration _config;
        private readonly IPaychexTokenCache _tokenCache;

        /// <summary>
        /// Dependency injection
        /// </summary>
        /// <param name="config"></param>
        /// <param name="tokenCache"></param>
        public PaychexClientFactory(IPaychexConfiguration config, IPaychexTokenCache tokenCache)
        {
            _config = config;
            _tokenCache = tokenCache;
        }

        /// <inheritdoc />
        public IRestClient CreateClient() => new RestClient();

        /// <inheritdoc />
        public IRestRequest CreateRequest() => new RestRequest();

        /// <inheritdoc />
        public IAuthenticator CreateAuthenticator() => new PaychexAuthenticator(_config, _tokenCache, this);
    }
}
