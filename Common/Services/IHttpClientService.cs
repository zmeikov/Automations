using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Common.Services
{
	public interface IHttpClientService : IDisposable
	{
		Task<HttpResponseMessage> SendAsync(HttpRequestMessage message);

		Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent httpContent);
		Task<HttpResponseMessage> GetAsync(string requestUri);		
		Task<HttpResponseMessage> DeleteAsync(string requestUri);
		Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent httpContent);
		Task<HttpResponseMessage> PatchAsync(string requestUri, HttpContent httpContent);
	}
}
