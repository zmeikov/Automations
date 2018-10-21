using System.Collections.Generic;
using Common.Models;

namespace Common.Services
{
	public interface IRestService
	{
		RestResult<T> Post<T>(string url, Dictionary<string, string> headers = null);
		RestResult<T> Post<T, TContent>(string url, TContent body, Dictionary<string, string> headers = null);
		RestResult<T> Get<T>(string url);
		RestResult<T> Get<T>(string url, Dictionary<string, string> headers);
		RestResult<T> Delete<T>(string url);
		RestResult<T> Patch<T, TContent>(string url, TContent body);
		RestResult<string> Put<TContent>(string url, TContent body);
		void SetAuthorizationHeader(string basic, string authorizationHeaderValue);
	}
}
