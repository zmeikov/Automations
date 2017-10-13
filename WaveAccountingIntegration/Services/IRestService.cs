using WaveAccountingIntegration.Models;

namespace WaveAccountingIntegration.Services
{
	public interface IRestService
	{
		RestResult<T> Post<T>(string url);
		RestResult<T> Post<T, TContent>(string url, TContent body);
		RestResult<T> Get<T>(string url);
		RestResult<T> Delete<T>(string url);
		RestResult<T> Patch<T, TContent>(string url, TContent body);
		RestResult<string> Put<TContent>(string url, TContent body);
		void SetAuthorizationHeader(string basic, string authorizationHeaderValue);
	}
}
