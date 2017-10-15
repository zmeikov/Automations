using System.Collections.Generic;
using System.Security.Authentication;
using System.Web.Mvc;
using WaveAccountingIntegration.Models;
using WaveAccountingIntegration.Services;

namespace WaveAccountingIntegration.Controllers
{
	public class BaseController : Controller
	{
		public IFileSettingsService _fileSettingsServiceService;
		public IRestService _restService;
		public ICustomerSettingsService _customerSettingsService;
		public IHeadersParser _headersParser;

		public AppSettings _appAppSettings;
		public Dictionary<string, string> _headers;

		public BaseController()
		{
			_fileSettingsServiceService = new FileSettingsServiceService();
			_appAppSettings = _fileSettingsServiceService.GetSettings();

			_restService = new RestService(new HttpClientServiceFactory());
			_restService.SetAuthorizationHeader("Bearer", _appAppSettings.Bearer);

			_customerSettingsService = new CustomerSettingsService();

			_headersParser = new HeadersParser();


			#region check for expired authorization
			var getUser = _restService.Get<User>("https://api.waveapps.com/user/");
			if (!getUser.IsSuccessStatusCode)
			{
				throw new InvalidCredentialException();
			}
			#endregion

			_headers = _headersParser.ParseHeadersFromFile(
				System.Web.Hosting.HostingEnvironment.MapPath(@"~/App_Data/RequestHeaders.txt"));
		}
	}
}