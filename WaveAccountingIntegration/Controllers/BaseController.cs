﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Common.Services;
using WaveAccountingIntegration.Models;
using WaveAccountingIntegration.Services;

namespace WaveAccountingIntegration.Controllers
{
	public class BaseController : Controller
	{
		public IFileSettingsService _fileSettingsServiceService;
		public IRestService _restService;
		public ICustomerService _customerService;
		public IHeadersParser _headersParser;
		public ISendGmail _sendGmail;

		public AppSettings _appSettings;
		public Dictionary<string, string> _headers;

		

		public BaseController()
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;

			_sendGmail = new SendGmail();
			_fileSettingsServiceService = new FileSettingsService();
			_appSettings = _fileSettingsServiceService.GetSettings<AppSettings>(System.Web.Hosting.HostingEnvironment.MapPath(@"~/App_Data/AppSettings.json"));

			

			_customerService = new CustomerService();

			_headersParser = new HeadersParser();
			_headers = _headersParser.ParseHeadersFromFile(System.Web.Hosting.HostingEnvironment.MapPath(@"~/App_Data/RequestHeaders.txt"));

			_restService = new RestService(new HttpClientServiceFactory());

			//This is now fetched via cookies parsing
			//_restService.SetAuthorizationHeader("Bearer", _appSettings.Bearer);
			#region get Bearer token from Cookies
			var cookies = _headers.SingleOrDefault(x => x.Key.ToUpper() == "COOKIE").Value.Split(';');
			var bearerAuthString = cookies.First(x => x.Contains("aveapps=")).Substring(10);

			_restService.SetAuthorizationHeader("Bearer", bearerAuthString);
			#endregion



			#region check for expired authorization
			var getUser = _restService.Get<User>("https://api.waveapps.com/user/");
			if (!getUser.IsSuccessStatusCode)
			{
				throw new InvalidCredentialException();
			}
			#endregion
		}


		public string ExtractEmailFromString(string input)
		{
			var RegexPattern = @"\b[A-Z0-9._-]+@[A-Z0-9][A-Z0-9.-]{0,61}[A-Z0-9]\.[A-Z.]{2,6}\b";

			var emailRegex = new Regex(RegexPattern, RegexOptions.IgnoreCase);

			return emailRegex.Match(input).ToString();
		}
	}
}