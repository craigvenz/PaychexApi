using System;
using System.Collections.Specialized;
using Paychex.Api.Api.Interfaces;

namespace Paychex.Api
{
    public sealed class PaychexConfiguration : ConfigurationCollection, IPaychexConfiguration
    {
        public string UrlEndpoint => GetSetting("Paychex_UrlEndpoint");
        public string ApiKey => GetSetting("Paychex_ApiKey");
        public string ClientSecret => GetSetting("Paychex_ClientSecret");
        public TimeSpan Timeout => TimeSpan.FromSeconds(GetSetting("Paychex_Timeout", 30));

        public PaychexConfiguration() : this(ConfigurationManager.AppSettings) { }
        public PaychexConfiguration(NameValueCollection settings) : base(settings) { }
    }
}
