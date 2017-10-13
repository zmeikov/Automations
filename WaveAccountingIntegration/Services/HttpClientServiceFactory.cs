using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web;

namespace WaveAccountingIntegration.Services
{
    public class HttpClientServiceFactory : IHttpClientServiceFactory
    {
        public IHttpClientService Create(string baseApiUrl, AuthenticationHeaderValue authenticationHeader = null,
            string accept = "application/json")
        {
            return new HttpClientService(baseApiUrl, authenticationHeader, accept);
        }
    }
}