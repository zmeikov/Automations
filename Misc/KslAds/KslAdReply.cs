using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Common.Services;
using Misc.Models;
using Newtonsoft.Json;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using Xunit;

namespace Misc.KslAds
{
	public class KslAdReply
	{
		private readonly string kslJsonPath = @"C:\dev\github\zmeikov\Automations\Misc\KslAds\KslUrls.json";
		private readonly string settingsJsonPath = @"C:\dev\github\zmeikov\Automations\Misc\App_Data\AppSettings.json";
		private readonly IFileSettingsService _settingsService;

		public KslAdReply()
		{
			_settingsService = new FileSettingsService();
		}

		[Fact]
		public void ColdSendSmsToAllAds()
		{
			
			var urls = JsonConvert.DeserializeObject<KslUrls>(File.ReadAllText(kslJsonPath));
			
			ChromeDriver chrome = null;

			var googleCreds = _settingsService.GetSettings<AppSettings>(settingsJsonPath).GoogleSettings;

			///*
			chrome = new ChromeDriver {Url = "https://www.google.com/voice/b/0#inbox" }; Thread.Sleep(2000);
			chrome.Manage().Window.Minimize(); Thread.Sleep(5000);

			//login to GV
			chrome.FindElementById("identifierId").SendKeys(googleCreds.UserName); Thread.Sleep(2000);
			chrome.FindElementsByTagName("content").First(w => w.Text == "Next").Click(); Thread.Sleep(2000);
			chrome.FindElementByName("password").SendKeys(googleCreds.Password); Thread.Sleep(2000);
			chrome.FindElementsByTagName("content").First(w => w.Text == "Next").Click(); Thread.Sleep(5000);
			//*/
			var processed = new List<string>();

			FirefoxDriver firefox = null;
			foreach (var url in urls.pending)
			{
				try
				{
					string sellerName = null;
					string sellerPhone = null;
					string itemName = null;
					string itemPrice = null;
					///*
					firefox = new FirefoxDriver {Url = url}; Thread.Sleep(2000);
					firefox.Manage().Window.Minimize(); Thread.Sleep(5000);

					//fill data form KSL classifeds template
					try {sellerName = firefox.FindElementByCssSelector("span.listingContactSeller-firstName-value").Text;} catch (Exception ex) { }
					try{sellerPhone = firefox.FindElementByCssSelector("span.listingContactSeller-optionText").Text.Replace("-", "");} catch (Exception ex) { }
					try{itemName = firefox.FindElementByCssSelector("h1.listingDetails-title").Text;} catch (Exception ex) { }
					try{itemPrice = firefox.FindElementByCssSelector("h2.listingDetails-price").Text;} catch (Exception ex) { }


					//fill data form KSL cars template
					if (string.IsNullOrWhiteSpace(sellerName))
					try {sellerName = firefox.FindElementByCssSelector("span.vdp-contact-text.first-name").Text; } catch (Exception ex) { }
					if(string.IsNullOrWhiteSpace(sellerPhone))
					try {sellerPhone = new string( firefox.FindElementByCssSelector("a.stupid-ios").Text.Where(Char.IsDigit).ToArray()); } catch (Exception ex) { }
					if(string.IsNullOrWhiteSpace(itemName))
					try {itemName = firefox.FindElementByCssSelector("h1.title").Text; } catch (Exception ex) { }
					if(string.IsNullOrWhiteSpace(itemPrice))
					try {itemPrice = firefox.FindElementByCssSelector("h3.price").Text; } catch (Exception ex) { }

					if (!decimal.TryParse(sellerPhone, out _))
						continue;

					//go to legacy GV
					chrome.Url = "https://voice.google.com/"; Thread.Sleep(2000);
					chrome.FindElementByCssSelector(".md-icon-button > .material-icons-extended").Click(); Thread.Sleep(2000);
					chrome.FindElementByXPath("//div/div/gv-nav-item/div/div").Click(); Thread.Sleep(2000);

					chrome.SwitchTo().Window(chrome.WindowHandles.Last()); Thread.Sleep(2000);

					//click new text
					var newtextButton = chrome.FindElementByXPath("//div[@id=\'gc-sidebar-jfk-container\']/div/div/div[2]");
					newtextButton.Click(); Thread.Sleep(2000);
					//newtextButton.SendKeys(sellerPhone);

					var phoneInput = chrome.FindElementById("gc-quicksms-number");
					phoneInput.Click(); Thread.Sleep(2000);
					phoneInput.SendKeys(sellerPhone); Thread.Sleep(2000);

					var message =
						$"Hi {sellerName}, I saw your KSL.com posting about {itemName.ToUpper()} for {itemPrice}. Link: {url} ; " +
						$"Please let me know if it is still available. Thank you for your time.";

					var messageInput = chrome.FindElementById("gc-quicksms-text2");
					messageInput.Click(); Thread.Sleep(2000);
					messageInput.SendKeys(message); Thread.Sleep(3000);

					var sendButton = chrome.FindElementByXPath("//div[@id=\'gc-quicksms-send2\']/div/div/div/div[2]");
					sendButton.Click(); Thread.Sleep(2500);
					//*/

					processed.Add(url);
				}
				catch (Exception ex)
				{
					Thread.Sleep(500);
				}
				finally
				{
					firefox?.Close();
					firefox?.Dispose();
				}
			
				
			}

			foreach (var url in processed)
			{
				urls.pending.Remove(url);
				urls.attempted.Add(url);
			}


			//save the empty list
			File.WriteAllText(kslJsonPath, JsonConvert.SerializeObject(urls));

			chrome?.Close();
			chrome?.Dispose();
		}

		public class KslUrls
		{
			public List<string> pending { get; set; }
			public List<string> attempted { get; set; }
		}
	}

	

}
