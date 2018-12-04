using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace BillPay.Poms
{
	public class BasePage
	{
		public readonly IWebDriver _driver;

		public BasePage(IWebDriver driver)
		{
			_driver = driver;
		}
		public void GoToUrlAndWaitToLoad(string url)
		{
			_driver.Url = url;

			var wait = new WebDriverWait(_driver, TimeSpan.FromMinutes(1));

			wait.Until(d => d.Url.Contains(url));
			wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
		}

		public void WaitForPageToLoad(string url)
		{
			var wait = new WebDriverWait(_driver, TimeSpan.FromMinutes(1));

			wait.Until(d => d.Url.Contains(url));
			wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
		}

	}


}
