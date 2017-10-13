using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace WaveAccountingIntegration.Services
{
    public class HttpClientService : IHttpClientService
    {
        private readonly HttpClient _httpClient;

        public HttpClientService(string baseApiUrl, AuthenticationHeaderValue authenticationHeader = null, string accept = "application/json")
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(baseApiUrl) };

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(accept));

            if (authenticationHeader != null)
            {
                _httpClient.DefaultRequestHeaders.Authorization = authenticationHeader;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent httpContent)
        {
            return _httpClient.PostAsync(requestUri, httpContent);
        }

        public Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            return _httpClient.GetAsync(requestUri);
        }

        public Task<HttpResponseMessage> DeleteAsync(string requestUri)
        {
            return _httpClient.DeleteAsync(requestUri);
        }

        public Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent httpContent)
        {
            var request = new HttpRequestMessage(new HttpMethod("PUT"), requestUri) { Content = httpContent };
            return _httpClient.SendAsync(request);
        }

        public Task<HttpResponseMessage> PatchAsync(string requestUri, HttpContent httpContent)
        {
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri) { Content = httpContent };
            return _httpClient.SendAsync(request);
        }
    }
}