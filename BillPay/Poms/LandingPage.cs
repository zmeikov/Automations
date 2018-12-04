using OpenQA.Selenium;

namespace BillPay.Poms
{
	public class LandingPage : BasePage
	{

		public const string PageUrl = "https://www.municipalonlinepayments.com/ghidut/utilities";

		public LandingPage(IWebDriver driver) : base(driver)
		{

		}

		public bool IsAt()
		{
			return _driver.Url == PageUrl;
		}
	}
}