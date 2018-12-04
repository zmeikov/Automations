using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using Xunit;
using Xunit.Sdk;

namespace Misc
{
	public class VSkeys
	{
		
		private readonly IWebDriver _driver;

		public VSkeys()
		{
			//_driver = new FirefoxDriver(new FirefoxOptions{ Profile = new FirefoxProfile("C:\\Users\\todor.peykov\\AppData\\Roaming\\Mozilla\\Firefox\\Profiles\\c7sclkex.default") });
			_driver = new ChromeDriver();
		}

		[Fact]
		public void ClaimKeys_WillSucceed()
		{
			//_driver.Url = "https://my.visualstudio.com/ProductKeys";
			//Thread.Sleep(5000);

			//_driver.FindElement(By.Name("loginfmt")).SendKeys("todor.peykov@autopoint.com"); Thread.Sleep(4000);
			//_driver.FindElement(By.Id("idSIButton9")).Click(); Thread.Sleep(4000);
			//_driver.FindElement(By.Id("aadTileTitle")).Click(); Thread.Sleep(4000);
			//_driver.FindElement(By.Name("passwd")).SendKeys("XXXXXX"); Thread.Sleep(4000);
			//_driver.FindElement(By.Id("idSIButton9")).Click(); Thread.Sleep(4000);
			//_driver.FindElement(By.Id("idSIButton9")).Click(); Thread.Sleep(15000);

			//var rand = new Random();

			//for (var i = 0; i < 20; i++)
			//{
			//	var links = _driver.FindElements(By.LinkText("Claim Key"));

			//	int randIndex = rand.Next(0, links.Count);
			//	links[randIndex].Click();

			//	Thread.Sleep(4000);
			//	_driver.Url = "https://my.visualstudio.com/ProductKeys";
			//	Thread.Sleep(8000);


			//}

			_driver.Close();
			_driver.Quit();
		}


	}

}
