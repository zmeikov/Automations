using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using BillPay.Poms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

namespace BillPay.Tests
{
	/// <summary>
	/// Summary description for CanLogIn
	/// </summary>
	[TestClass]
	public class UserAuthentication
	{
		private static IWebDriver _driver;
		private static LoginPage _loginPage;
		private static LandingPage _landingPage;
		
		public UserAuthentication()
		{
			_driver = new FirefoxDriver();
			_driver.Manage().Window.Minimize();
			_loginPage = new LoginPage(_driver);
		}

		[TestMethod]
		public void CanLogIn()
		{
			_loginPage.GoTo();
			_driver.Url.Should().Be(LoginPage.PageUrl);

			_loginPage.FillUserPassword();
			_landingPage = _loginPage.Login();

			_driver.Url.Should().Be(LandingPage.PageUrl);
			_landingPage.IsAt().Should().BeTrue();
		}


		[ClassCleanup]
		public static void Cleanup()
		{
			_driver.Close();
		}
	}
}
