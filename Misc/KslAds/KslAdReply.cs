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

			ChromeDriver chromeGv = null;

			var googleCreds = _settingsService.GetSettings<AppSettings>(settingsJsonPath).GoogleSettings;
			try
			{
				///*
				chromeGv = new ChromeDriver {Url = "https://www.google.com/voice/b/0#inbox" };
				Thread.Sleep(2000);
				if(DateTime.Now.Date > DateTime.Parse("2019-11-17"))
					chromeGv.Manage().Window.Minimize();
				Thread.Sleep(5000);

				//login to GV
				chromeGv.FindElementById("identifierId").SendKeys(googleCreds.UserName);
				Thread.Sleep(2000);
				chromeGv.FindElementByCssSelector(".RveJvd").Click();
				Thread.Sleep(5000);
				chromeGv.FindElementByName("password").SendKeys(googleCreds.Password);
				Thread.Sleep(2000);
				chromeGv.FindElementByCssSelector("#passwordNext .RveJvd").Click();
				Thread.Sleep(5000);
				//*/
				var processed = new List<string>();

				ChromeDriver chromeKsl = null;
				foreach (var url in urls.pending)
				{
					try
					{
						string sellerName = null;
						string sellerPhone = null;
						string itemName = null;
						string itemPrice = null;
						///*
						chromeKsl = new ChromeDriver { Url = url};
						Thread.Sleep(2000);
						if (DateTime.Now.Date > DateTime.Parse("2019-11-17"))
							chromeKsl.Manage().Window.Minimize();
						Thread.Sleep(5000);

						//fill data form KSL classifeds template
						try
						{
							sellerName = chromeKsl.FindElementByCssSelector("span.listingContactSeller-firstName-value")
								.Text;
						}
						catch (Exception ex)
						{
						}

						try
						{
							sellerPhone = chromeKsl.FindElementByCssSelector("span.listingContactSeller-optionText").Text
								.Replace("-", "");
						}
						catch (Exception ex)
						{
						}

						try
						{
							itemName = chromeKsl.FindElementByCssSelector("h1.listingDetails-title").Text;
						}
						catch (Exception ex)
						{
						}

						try
						{
							itemPrice = chromeKsl.FindElementByCssSelector("h2.listingDetails-price").Text;
						}
						catch (Exception ex)
						{
						}


						//fill data form KSL cars template
						if (string.IsNullOrWhiteSpace(sellerName))
							try
							{
								sellerName = chromeKsl.FindElementByCssSelector("span.vdp-contact-text.first-name").Text;
							}
							catch (Exception ex)
							{
							}

						if (string.IsNullOrWhiteSpace(sellerPhone))
							try
							{
								sellerPhone = new string(chromeKsl.FindElementByCssSelector("a.stupid-ios").Text
									.Where(Char.IsDigit).ToArray());
							}
							catch (Exception ex)
							{
							}

						if (string.IsNullOrWhiteSpace(itemName))
							try
							{
								itemName = chromeKsl.FindElementByCssSelector("h1.title").Text;
							}
							catch (Exception ex)
							{
							}

						if (string.IsNullOrWhiteSpace(itemPrice))
							try
							{
								itemPrice = chromeKsl.FindElementByCssSelector("h3.price").Text;
							}
							catch (Exception ex)
							{
							}

						if (!decimal.TryParse(sellerPhone, out _))
							continue;

						//go to legacy GV
						//chromeGv.Url = "https://voice.google.com/";
						//Thread.Sleep(2000);
						//chromeGv.FindElementByCssSelector(".gb_tc:nth-child(1) path").Click();
						//Thread.Sleep(2000);
						//chromeGv.FindElementByCssSelector(".hide-xs .navList > div .navItemLabel").Click();
						//Thread.Sleep(2000);
						//chromeGv.SwitchTo().Window(chromeGv.WindowHandles.Last());
						//Thread.Sleep(2000);

						//go to new GV sms inbox
						chromeGv.Url = "https://voice.google.com/u/0/messages";
						//click new text
						var newtextButton =
							chromeGv.FindElementByCssSelector(".gmat-subhead-2");
						newtextButton.Click();
						Thread.Sleep(2000);
						//newtextButton.SendKeys(sellerPhone);

						var phoneInput = chromeGv.FindElementByCssSelector(".mLSKFd");
						phoneInput.Click();
						Thread.Sleep(2000);
						phoneInput.SendKeys(sellerPhone);
						Thread.Sleep(2000);

						var message =
							$"Hi {sellerName}, I saw your KSL.com posting about {itemName.ToUpper()} for {itemPrice}. Link: {url} ; " +
							$"Please let me know if it is still available. Thank you for your time.";

						var messageInput = chromeGv.FindElementById("input_2");
						messageInput.Click();
						Thread.Sleep(2000);
						messageInput.SendKeys(message);
						Thread.Sleep(3000);

						var sendButton =
							chromeGv.FindElementByXPath("//div[@id=\'gc-quicksms-send2\']/div/div/div/div[2]");
						sendButton.Click();
						Thread.Sleep(2500);
						//*/

						processed.Add(url);
					}
					catch (Exception ex)
					{
						Thread.Sleep(500);
					}
					finally
					{
						chromeKsl?.Close();
						chromeKsl?.Dispose();
					}


				}

				foreach (var url in processed)
				{
					urls.pending.Remove(url);
					urls.attempted.Add(url);
				}


				//save the empty list
				File.WriteAllText(kslJsonPath, JsonConvert.SerializeObject(urls));
			}
			catch (Exception ex)
			{
				Thread.Sleep(500);
			}
			finally
			{
				chromeGv?.Close();
				chromeGv?.Dispose();
			}
		}

		public class KslUrls
		{
			public List<string> pending { get; set; }
			public List<string> attempted { get; set; }
		}
	}

	

}
