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


		public AppSettings _appAppSettings;

		public BaseController()
		{
			_fileSettingsServiceService = new FileSettingsServiceService();
			_appAppSettings = _fileSettingsServiceService.GetSettings();

			_restService = new RestService(new HttpClientServiceFactory());
			_restService.SetAuthorizationHeader("Bearer", _appAppSettings.Bearer);

			_customerSettingsService = new CustomerSettingsService();
		}
	}
}