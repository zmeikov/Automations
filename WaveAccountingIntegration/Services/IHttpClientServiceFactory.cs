using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace WaveAccountingIntegration.Services
{
    public interface IHttpClientServiceFactory
    {
        IHttpClientService Create(string baseApiUrl, AuthenticationHeaderValue authenticationHeader = null, string accept = "application/json");
        IHttpClientService Create(string baseApiUrl, Dictionary<string, string> headers);
    }
}
