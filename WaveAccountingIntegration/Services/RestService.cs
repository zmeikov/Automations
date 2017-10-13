using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using WaveAccountingIntegration.Models;
using WebGrease;

namespace WaveAccountingIntegration.Services
{
    public class RestService : IRestService
    {
        private readonly IHttpClientServiceFactory _httpClientServiceFactory;
        private AuthenticationHeaderValue _authenticationHeader;

        public RestService(IHttpClientServiceFactory httpClientServiceFactory)
        {
            _httpClientServiceFactory = httpClientServiceFactory;
        }

        public void SetAuthorizationHeader(string scheme, string parameter)
        {
            _authenticationHeader = new AuthenticationHeaderValue(scheme, parameter);
        }

        public RestResult<T> Post<T>(string url)
        {
            return Post<T, string>(url, string.Empty);
        }

        public RestResult<T> Post<T, TContent>(string url, TContent body)
        {
            RestResult<T> result;

            using (var httpClient = _httpClientServiceFactory.Create(url, _authenticationHeader))
            {
                var httpContent = new StringContent(JsonConvert.SerializeObject(body),Encoding.UTF8, "application/json");

                var response = httpClient.PostAsync(string.Empty, httpContent).Result;
                result = GetResult<T>(response);
            }

            return result;
        }

        public RestResult<T> Get<T>(string url)
        {
            RestResult<T> result;

            using (var httpClient = _httpClientServiceFactory.Create(url, _authenticationHeader))
            {
                var response = httpClient.GetAsync(string.Empty).Result;
                result = GetResult<T>(response);

            }

            return result;
        }

        public RestResult<T> Delete<T>(string url)
        {
            RestResult<T> result;

            using (var httpClient = _httpClientServiceFactory.Create(url, _authenticationHeader))
            {
                var response = httpClient.DeleteAsync(string.Empty).Result;

                result = GetResult<T>(response);
            }

            return result;
        }

        public RestResult<T> Patch<T, TContent>(string url, TContent body)
        {
            RestResult<T> result;

            using (var httpClient = _httpClientServiceFactory.Create(url, _authenticationHeader))
            {
                var httpContent = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

                var response = httpClient.PatchAsync(string.Empty, httpContent).Result;

                result = GetResult<T>(response);

            }

            return result;
        }

        public RestResult<string> Put<TContent>(string url, TContent body)
        {
            RestResult<string> result;

            using (var httpClient = _httpClientServiceFactory.Create(url, _authenticationHeader))
            {
                var httpContent = new StringContent(JsonConvert.SerializeObject(body));
                
                var response = httpClient.PutAsync(string.Empty, httpContent).Result;


                var content = response.Content.ReadAsStringAsync().Result;
                result = new RestResult<string>
                {
                    StatusCode = (int)response.StatusCode
                };

                if (result.IsSuccessStatusCode)
                {
                    result.Result = content;
                }
                else
                {
                    result.ErrorMessage = content;
                }

            }

            return result;
        }

        private RestResult<T> GetResult<T>(HttpResponseMessage response)
        {
            var content = response.Content.ReadAsStringAsync().Result;

            var result = new RestResult<T>
            {
                StatusCode = (int)response.StatusCode
            };

            if (result.IsSuccessStatusCode)
            {
                result.Result = JsonConvert.DeserializeObject<T>(content);
            }
            else
            {
                result.ErrorMessage = content;
            }

            return result;
        }

    }

    public enum RestAction
    {
        POST,
        PUT,
        DELETE,
        GET,
        PATCH
    }
}