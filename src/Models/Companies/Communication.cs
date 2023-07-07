// LcpMvc.Lcp.Paychex.Communication.cs
// Craig Venz - 09/26/2019 - 2:33 PM

namespace Paychex.Api.Models.Companies {
    /// <summary>
    /// Communications object for companies. Identical to base class except EF navigation.
    /// </summary>
    public class Communication : Common.Communication
    {
        /// <summary>
        /// For EF navigation property mapping. Currently not used.
        /// </summary>
        [JsonIgnore]
        public Company Company { get; set; }

        /// <summary>
        /// Create without parameters - allowed.
        /// </summary>
        public Communication() { }

        /// <summary>
        /// Copy from an existing common communication.
        /// </summary>
        /// <param name="other">Other communication object</param>
        public Communication(Common.Communication other)
        {
            CopyFrom(other);
        }

    }
}