// LcpMvc.Lcp.Services.Tests.PaychexConfigTests.cs
// Craig Venz - 10/22/2019 - 5:04 PM

using System;
using System.Collections.Specialized;
using FluentAssertions;
using Lcp.Services.Paychex;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lcp.Services.Tests.Paychex 
{
    public class PaychexConfigTests 
    {
        [TestMethod]
        public void Configs()
        {
            var nvc = new NameValueCollection
            {
                { "Paychex_UrlEndpoint", "test" },
                { "Paychex_ApiKey", "1234567890" },
                { "Paychex_ClientSecret", "9876543210" },
                { "Paychex_Timeout", "60" }
            };
            var config = new PaychexConfiguration(nvc);

            config.ApiKey.Should()
                  .Be("1234567890");
            config.UrlEndpoint.Should()
                  .Be("test");
            config.ClientSecret.Should()
                  .Be("9876543210");
            config.Timeout.Should()
                  .Be(TimeSpan.FromSeconds(60));

            nvc.Set("Paychex_Timeout", "garbage");
            config.Timeout.Should()
                  .Be(TimeSpan.FromSeconds(30));
        }
    }
}