using System;
using System.Collections.Generic;
using System.ComponentModel;
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
		private Dictionary<string, string> _headers;

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

		public HttpClientService(string baseApiUrl, Dictionary<string, string> headers)
		{
			var handler = new HttpClientHandler {UseCookies = false};

			_httpClient = new HttpClient(handler) { BaseAddress = new Uri(baseApiUrl) };

			_httpClient.DefaultRequestHeaders.Accept.Clear();
			if (headers == null)
				throw new InvalidEnumArgumentException("headres cannot be null");

			foreach (var header in headers)
			{
				_httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
			}

		}

		public void Dispose()
		{
			_httpClient?.Dispose();
		}

		public Task<HttpResponseMessage> SendAsync(HttpRequestMessage message)
		{
			return _httpClient.SendAsync(message);
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