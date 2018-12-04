using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace BillPay.Poms
{
	public class LoginPage : BasePage
	{
		
		public const string PageUrl = "https://www.municipalonlinepayments.com/ghidut/login?returnUrl=%2Fghidut%2Futilities";

		public LoginPage(IWebDriver driver) : base (driver)
		{
			
		}


		public void GoTo()
		{
			GoToUrlAndWaitToLoad(PageUrl);
		}

		public void FillUserPassword()
		{
			_driver.FindElement(By.Id("UserName")).SendKeys("zmeikov@yahoo.com");
			_driver.FindElement(By.Id("Password")).SendKeys("992018");

		}

		public LandingPage Login()
		{
			_driver.FindElement(By.CssSelector("button.btn-primary.btn")).Click();

			WaitForPageToLoad(LandingPage.PageUrl);

			return new LandingPage(_driver);
		}
	}
}
