// LcpMvc.Lcp.Paychex.IRestClientFactory.cs
// Craig Venz - 09/25/2019 - 9:54 AM

using RestSharp;
using RestSharp.Authenticators;

namespace Paychex.Api.Api.Interfaces {
    /// <summary>
    /// Abstraction for creating concrete classes of RestSharp classes. Used for unit testing.
    /// </summary>
    public interface IRestClientFactory
    {
        /// <summary>
        /// Instantiate an <see cref="IRestClient">IRestClient</see>.
        /// </summary>
        /// <returns>Implementation of an IRestClient.</returns>
        IRestClient CreateClient();

        /// <summary>
        /// Instantiate an <see cref="IRestRequest">IRestRequest</see>.
        /// </summary>
        /// <returns>Implementation of an IRestRequest.</returns>
        RestRequest CreateRequest();

        /// <summary>
        /// Instantiate an <see cref="IAuthenticator">IAuthenticator</see>.
        /// </summary>
        /// <returns>Implementation of an IAuthenticator</returns>
        IAuthenticator CreateAuthenticator();
    }
}