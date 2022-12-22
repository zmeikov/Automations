using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml.Schema;
using Microsoft.Ajax.Utilities;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using WaveAccountingIntegration.Models;


namespace WaveAccountingIntegration.Controllers
{
	public class BusinessProcessingMahayagController : BaseController
	{
		private static MemoryCache memoryCache;

		public ActionResult AutoPinResetAndText(DayOfWeek day = DayOfWeek.Sunday)
		{
			var messages = new ConcurrentBag<string>();
			var testDateString = "2018-09-22";

			if (DateTime.Today.Date <= DateTime.Parse(testDateString))
				day = DayOfWeek.Saturday;

			if (DateTime.Today.DayOfWeek == day && DateTime.Now.Hour < 14 && DateTime.Now.Hour > 8)
			{
				var propertyIds = _appSettings.MahayagAddresses.Select(x => x.Id);

				if (DateTime.Today.Date >= DateTime.Parse(testDateString))
					propertyIds = propertyIds.Where(x => x == "0000");

				var allActiveCustomers = GetActiveCustomers();

				//Parallel.ForEach(propertyIds, id =>
				//{
				foreach (var id in propertyIds.ToList())
				{

					var customersAtThisAddress = allActiveCustomers.Where(x => x.name.StartsWith(id));
					var customerSettingsAtThisAddress = new Dictionary<Customer, CustomerSettings>();

					foreach (var cust in customersAtThisAddress.ToList())
					{
						customerSettingsAtThisAddress.Add(cust, _customerService.ExctractSettingsFromCustomerObject(cust));
					}

					//check if someoene got their pin reset today
					var pinResetsToday = customerSettingsAtThisAddress.Where(x => x.Value.LastPinResetDate?.Date == DateTime.Today.Date);
					if (pinResetsToday.Any())
					{
						messages.Add($"Pin reset for customer:{pinResetsToday.First().Key.name} already occured today on preperty Id: {id};");
					}
					else
					{
						var customer = customerSettingsAtThisAddress.OrderBy(x => x.Value.LastPinResetDate).First().Key;
						var custSettings = _customerService.ExctractSettingsFromCustomerObject(customer);

						if (custSettings.LastPinResetDate >= DateTime.Today.Date.AddMonths(-1))
						{
							messages.Add($"Skipping Pin reset for customer:{pinResetsToday.First().Key.name} since it already occured on {custSettings.LastPinResetDate}, and is less than a month ago");
						}
						else
						{
							custSettings.LastPinResetDate = DateTime.Today;

							var newPin = new Random().Next(0000, 9919).ToString("0000");
							var oldPin = customer.phone_number;

							if (_customerService.SaveUpdatedCustomerSettings(customer, custSettings, _restService).IsSuccessStatusCode)
							{
								if (_customerService.SaveUpdatedCustomerPhone(customer, newPin, _restService).IsSuccessStatusCode)
								{
									//sent alert to name 1
									if (!string.IsNullOrWhiteSpace(ExtractEmailFromString(customer.address1)))
									{
										var name = customer.first_name.ToUpper().Trim();
										var body = GetAutoPinResetAndTextSmsAlertBody(name, oldPin, newPin);

										messages.Add($"alerting new/old pin: {newPin}/{oldPin} for customer:{name} on {ExtractEmailFromString(customer.address1)}");
										_sendGmail.SendSMS(ExtractEmailFromString(customer.address1), body, _appSettings.GoogleSettings);
									}

									//sent alert to name 2
									if (!string.IsNullOrWhiteSpace(ExtractEmailFromString(customer.address2)))
									{
										var name = customer.last_name.ToUpper().Trim();
										var body = GetAutoPinResetAndTextSmsAlertBody(name, oldPin, newPin);

										messages.Add($"alerting new/old pin: {newPin}/{oldPin} for customer:{name} on {ExtractEmailFromString(customer.address1)}");
										_sendGmail.SendSMS(ExtractEmailFromString(customer.address2), body, _appSettings.GoogleSettings);
									}
								}
							}
						}
						
					}

				}
				//});
			}
			else
			{
				messages.Add($"Skip Pin Reset since DateTime.Now.Hour={DateTime.Now.Hour}; DateTime.Today.DayOfWeek={DateTime.Today.DayOfWeek.ToString()};");
			}

			ViewBag.Message = string.Join(Environment.NewLine, messages);
			return View();
		}

		public ActionResult SmsAlertLateCustomers(int daysBetweenAlerts = 3, ulong narrowByCustomerId = 0)
		{
			var lateCustomers = GetLateCustomers(narrowByCustomerId);
			var messages = new ConcurrentBag<string>();

			var customersWithBalance = lateCustomers.Where(w => w.Value.ending_balance > 0);

			//Parallel.ForEach(customersWithBalance, (customerKvp) =>
			foreach(var customerKvp in customersWithBalance)
			{
				var customer = customerKvp.Key;
				var custSettings = _customerService.ExctractSettingsFromCustomerObject(customer);

				var daysSinceLastSmsAlert = (DateTime.Now - (custSettings.LastSmsAlertSent?? DateTime.Now)).Days;

				var minDaysBetweenAlerts = Math.Max(custSettings.CustomDaysBetweenSmsAlerts?? daysBetweenAlerts, daysBetweenAlerts);

				var lastPayment = customerKvp.Value.events.Where(x => x.event_type == "payment").OrderByDescending(x => x.date).FirstOrDefault();
				var lastInvoice = customerKvp.Value.events.Where(x => x.event_type == "invoice" && x.total > 0).OrderByDescending(x => x.date).First();
				var invoiceDue = _restService.Get<Invoice>(lastInvoice.invoice.url).Result;
				var daysSinceLastPayment = (DateTime.Now - (lastPayment?.date?? DateTime.Now.AddYears(-10))).Days;

				if (
					daysSinceLastSmsAlert >= minDaysBetweenAlerts &&
					daysSinceLastPayment >= 7 &&
					lastInvoice.date <= DateTime.Today.Date.AddDays(-5) &&
					DateTime.Now.Hour > 7 &&
					DateTime.Now.Hour < 13 &&
					DateTime.Now.DayOfWeek != DayOfWeek.Saturday &&
					DateTime.Now.DayOfWeek != DayOfWeek.Sunday &&
					custSettings.SendSmsAlerts == true &&
					customerKvp.Value.ending_balance >= 25
				)
				{
					//sent alert to name 1
					if (!string.IsNullOrWhiteSpace(ExtractEmailFromString(customer.address1)))
					{
						var name = customer.first_name.ToUpper().Trim();
						var body = GetLateCustomerSmsAlertBody(name, customerKvp, lastPayment, invoiceDue, custSettings);
						           
						messages.Add($"alerting late customer:{name} on {ExtractEmailFromString(customer.address1)}");
						_sendGmail.SendSMS(ExtractEmailFromString(customer.address1), body, _appSettings.GoogleSettings);
					}

					//sent alert to name 2
					if (!string.IsNullOrWhiteSpace(ExtractEmailFromString(customer.address2)))
					{
						var name = customer.last_name.ToUpper().Trim();
						var body = GetLateCustomerSmsAlertBody(name, customerKvp, lastPayment, invoiceDue, custSettings);

						messages.Add($"alerting late customer: {name} on {ExtractEmailFromString(customer.address2)}");
						_sendGmail.SendSMS(ExtractEmailFromString(customer.address2), body, _appSettings.GoogleSettings);
					}

					custSettings.LastSmsAlertSent = DateTime.Now;
					_customerService.SaveUpdatedCustomerSettings(customer, custSettings, _restService);

				}
				else
				{
					//skip alert
					messages.Add($"Skipping SmsAlert LastSmsAlertSent: {(custSettings.LastSmsAlertSent.HasValue ? custSettings.LastSmsAlertSent.Value.ToUSADateFormat() : string.Empty)}, " +
					             $"minDaysBetweenAlerts: {minDaysBetweenAlerts:00}, " +
					             $"lastInvoice.date: {lastInvoice.date.Value.ToUSADateFormat()}, " +
					             $"daysSinceLastPayment: {daysSinceLastPayment}, " +
					             $"SendSmsAlerts: {custSettings.SendSmsAlerts}, " +
					             $"ending_balance: {customerKvp.Value.ending_balance}, < $25" +
					             $"for: {customerKvp.Key.name}.");
				}
			}
			//});


			ViewBag.Message = string.Join(Environment.NewLine, messages);
			return View();
		}

		public ActionResult BrodcastAlertCustomers(string message, string nameStartsWith)
		{
			if (string.IsNullOrEmpty(message))
			{
				ViewBag.displayForm = true;
				return View();
			}

			var customersToAlert = GetActiveCustomers().Where(w => !w.name.StartsWith("?"));

			if (!string.IsNullOrWhiteSpace(nameStartsWith))
			{
				customersToAlert = customersToAlert.Where(w => w.name.StartsWith(nameStartsWith));
			}

			var messages = new ConcurrentBag<string>();

			Parallel.ForEach(customersToAlert, (customer) =>
			{
				var custSettings = _customerService.ExctractSettingsFromCustomerObject(customer);

				if 
				(
					custSettings.SendSmsAlerts == true
				)
				{
					//sent alert to name 1
					if (!string.IsNullOrWhiteSpace(ExtractEmailFromString(customer.address1)))
					{
						var name = customer.first_name.ToUpper().Trim();
						var body = $"Hello {name}, {message}";

						messages.Add($"BrodcastAlert customer:{name} on {ExtractEmailFromString(customer.address1)}");
						_sendGmail.SendSMS(ExtractEmailFromString(customer.address1), body, _appSettings.GoogleSettings);
					}

					//sent alert to name 2
					if (!string.IsNullOrWhiteSpace(ExtractEmailFromString(customer.address2)))
					{
						var name = customer.last_name.ToUpper().Trim();
						var body = $"Hello {name}, {message}";

						messages.Add($"BrodcastAlert customer: {name} on {ExtractEmailFromString(customer.address2)}");
						_sendGmail.SendSMS(ExtractEmailFromString(customer.address2), body, _appSettings.GoogleSettings);
					}

					custSettings.LastSmsAlertSent = DateTime.Now;
					_customerService.SaveUpdatedCustomerSettings(customer, custSettings, _restService);

				}
				else
				{
					//skip alert
					messages.Add($"Skipping BrodcastAlert Customer: SendSmsAlerts: {custSettings.SendSmsAlerts}, " +
								 $"for: {customer.name}.");
				}
			});


			ViewBag.Message = string.Join(Environment.NewLine, messages);
			return View();
		}

		public ActionResult EvictionDocs(ulong id, string form)
		{
			var customerStatement = new Dictionary<Customer, Transaction_History>();

			var customer = _restService.Get<Customer>(
				$"https://api.waveapps.com/businesses/{_appSettings.MahayagBusinessGuid}/customers/{id}/").Result;
			
			var statement = GetStatement(id, string.IsNullOrWhiteSpace(form) || form.Contains("Court"));

			var trxHistory = statement.transaction_history.FirstOrDefault();

			if (trxHistory != null)
				customerStatement.Add(customer, trxHistory);

			var addressId = customer.name.ToUpper().Replace("XXXX", "").Substring(0, customer.name.ToUpper().Replace("XXXX", "").IndexOf('-'));
			var address = _appSettings.MahayagAddresses.FirstOrDefault(x => x.Id == addressId);
			//if (customer.name.Contains('#'))
			//{
			//	var ponudPos = customer.name.IndexOf('#');
			//	var dashPos = customer.name.IndexOf('-');

			//	var diff = dashPos - ponudPos;

			//	address.Address1 += $" APT # {customer.name.Substring(ponudPos+1, diff - 1)}";
			//}

			List<Tennant> tenants = GetTennatsFromName(customer.name);

			ViewBag.address = address;
			ViewBag.CustomerSettings = _customerService.ExctractSettingsFromCustomerObject(customer);
			ViewBag.AppSettings = _appSettings;
			ViewBag.Tenants = tenants;
			var total = customerStatement.First().Value.ending_balance;
			ViewBag.customerName = customer.name;
			var customernames = string.Join(" + ", GetTennatsFromName(customer.name));
			ViewBag.Title = $"{customernames} {DateTime.Now.Date.ToISODateFormat()}";
			//ViewBag.invoice = customerStatement.Values.First().events.First(x => 
			//	( 
			//		//x.invoice.invoice_amount_due == total
			//		//|| 
			//		x.invoice.invoice_amount_due > 0
			//	) 
			//	&& x.event_type == "invoice")
			//	.invoice;
			//var invoice = _restService.Get<Invoice>(ViewBag.invoice.url + "?embed_items=true").Result;
			//ViewBag.invoice_items = invoice.items;
			ViewBag.EndOfLeaseDate = customerStatement.Values.First().events.OrderByDescending(x=>x.date).First(x => x.event_type == "invoice").invoice.invoice_date.GetEndOfLeaseDate().ToUSADateFormat();

			if (form == null)
				return View(customerStatement);
			else
			{
				return View(form, customerStatement);
			}
		}
		
		public ActionResult LateCustomers()
		{
			return View(GetLateCustomers());
		}

		public ActionResult RefreshBankConnections()
		{
			var refreshedSites = new ConcurrentBag<Connected_Site>();
			var Guid = _appSettings.PersonalGuid;

			var connectedSites = _restService.Get<List<Connected_Site>>(
				$"https://integrations.waveapps.com/{Guid}/bank/connected-sites", _headers).Result;
			if (connectedSites != null)
			{
				Parallel.ForEach(connectedSites, (site) =>
				//foreach (var site in connectedSites)
				{
					_restService.Post<string, object>($"https://integrations.waveapps.com/{Guid}/bank/refresh-accounts/{site.id}", null, _headers);

					for (int i = 0; i <= 5; i++)
					{
						Thread.Sleep(3000);
						_restService.Get<object>($"https://integrations.waveapps.com/{Guid}/bank/connected-sites/{site.id}?q=", _headers);
					}

					_restService.Get<object>($"https://integrations.waveapps.com/{Guid}/bank/mfa-communication/{site.id}", _headers);

					for (int i = 0; i <= 5; i++)
					{
						Thread.Sleep(3000);
						_restService.Get<object>($"https://integrations.waveapps.com/{Guid}/bank/connected-sites/{site.id}?q=", _headers);
					}

					refreshedSites.Add(site);
					Thread.Sleep(250);
				});
			}

			ViewBag.Message = "RefreshBankConnections";
			return View(refreshedSites);
		}

		public ActionResult SetCustomerDefaults()
		{
			var  messages =  new ConcurrentBag<string>();

			var processedCsutomers = new List<Customer>();

			var allCustomers = _restService.Get<List<Customer>>(
				$"https://api.waveapps.com/businesses/{_appSettings.MahayagBusinessGuid}/customers/").Result;

			var activeCustomersToSetup = allCustomers.Where(x => x.active && !x.name.StartsWith("XX") && !x.name.StartsWith("??"));

			//Parallel.ForEach(activeCustomersToSetup, (customer) =>
			foreach(var customer in activeCustomersToSetup)
			{
				var custSettings = _customerService.ExctractSettingsFromCustomerObject(customer);

				var defaultCustSettings = new CustomerSettings()
				{
					ChargeLateFee = false,
					NextLateFeeChargeDate = DateTime.Today,
					LateFeePercentRate = (decimal)0.02,
					LateFeeDailyAmount = 10,
					LateFeeChargeAboveBalance = 340,
					ConsolidateInvoices = true,
					SignedLeaseAgreement = false,
					LastPinResetDate = DateTime.Today.AddDays(-10),
					LastSmsAlertSent = DateTime.Today.AddDays(-10),
					LastTrashSmsAlertSent = DateTime.Today.AddDays(-10),
					CustomDaysBetweenSmsAlerts = 5,
					SendSmsAlerts = true,
					SendTrashSmsAlerts = true,
					HideLastPaymentDetails = false,
					HideStatementUrl = false,
					StatementUrl = "StatementUrl",
					EvictonNoticeDate = DateTime.Parse("2000-01-01"),
					EvictionCourtCaseNumber = "________",
					EvictionCourtAssignedJudge = "________"
				};

				if (custSettings == null)
				{
					//add new default settings to new customer settings
					customer.shipping_details = new Shipping_details()
					{
						delivery_instructions = JsonConvert.SerializeObject(defaultCustSettings),

						ship_to_contact = $"{customer.name}",
						phone_number = String.Empty,
						address1 = String.Empty,
						address2 = String.Empty,
						city = String.Empty,
						postal_code = String.Empty,
					};

					var updatedCustomerResult =
						_restService.Patch<UpdateCustomerResult, Customer>(customer.url, customer);

					if (updatedCustomerResult.IsSuccessStatusCode == false)
					{
						throw new InvalidOperationException("SetCustomerDefaults failed");
					}

					processedCsutomers.Add(customer);
					messages.Add($"Creating new default settings for customer: {customer.name}");

				}
				else
				{
					var changesMade = false;

					if (custSettings.ChargeLateFee == null) { custSettings.ChargeLateFee = defaultCustSettings.ChargeLateFee; messages.Add($"Setting default value ChargeLateFee to {custSettings.ChargeLateFee} for customer: {customer.name}"); changesMade = true; }
					if (custSettings.NextLateFeeChargeDate == null) { custSettings.NextLateFeeChargeDate = defaultCustSettings.NextLateFeeChargeDate; messages.Add($"Setting default value NextLateFeeChargeDate to {custSettings.NextLateFeeChargeDate} for customer: {customer.name}"); changesMade = true; }
					if (custSettings.LateFeePercentRate == null) { custSettings.LateFeePercentRate = defaultCustSettings.LateFeePercentRate; messages.Add($"Setting default value LateFeePercentRate to {custSettings.LateFeePercentRate} for customer: {customer.name}"); changesMade = true; }
					if (custSettings.LateFeeDailyAmount == null) { custSettings.LateFeeDailyAmount = defaultCustSettings.LateFeeDailyAmount; messages.Add($"Setting default value LateFeeDailyAmount to {custSettings.LateFeeDailyAmount} for customer: {customer.name}"); changesMade = true; }
					if (custSettings.LateFeeChargeAboveBalance == null) { custSettings.LateFeeChargeAboveBalance = defaultCustSettings.LateFeeChargeAboveBalance; messages.Add($"Setting default value LateFeeChargeAboveBalance to {custSettings.LateFeeChargeAboveBalance} for customer: {customer.name}"); changesMade = true; }
					//set NextLateFeeChargeDate to today if  ChargeLateFee is false
					if (custSettings.ChargeLateFee == false && custSettings.NextLateFeeChargeDate < DateTime.Today) { custSettings.NextLateFeeChargeDate = defaultCustSettings.NextLateFeeChargeDate; messages.Add($"Setting todays date for NextLateFeeChargeDate to {custSettings.NextLateFeeChargeDate} for customer: {customer.name}"); changesMade = true; }
					
					if (custSettings.ConsolidateInvoices == null) { custSettings.ConsolidateInvoices = defaultCustSettings.ConsolidateInvoices; messages.Add($"Setting default value ConsolidateInvoices to {custSettings.ConsolidateInvoices} for customer: {customer.name}"); changesMade = true; }
					if (custSettings.SignedLeaseAgreement == null) { custSettings.SignedLeaseAgreement = defaultCustSettings.SignedLeaseAgreement; messages.Add($"Setting default value SignedLeaseAgreement to {custSettings.SignedLeaseAgreement} for customer: {customer.name}"); changesMade = true; }
					if (custSettings.LastPinResetDate == null) { custSettings.LastPinResetDate = defaultCustSettings.LastPinResetDate; messages.Add($"Setting default value LastPinResetDate to {custSettings.LastPinResetDate} for customer: {customer.name}"); changesMade = true; }

					if (custSettings.LastSmsAlertSent == null) { custSettings.LastSmsAlertSent = defaultCustSettings.LastSmsAlertSent; messages.Add($"Setting default value LastSmsAlertSent to {custSettings.LastSmsAlertSent} for customer: {customer.name}"); changesMade = true; }
					if (custSettings.LastTrashSmsAlertSent == null) { custSettings.LastTrashSmsAlertSent = defaultCustSettings.LastTrashSmsAlertSent; messages.Add($"Setting default value LastTrashSmsAlertSent to {custSettings.LastTrashSmsAlertSent} for customer: {customer.name}"); changesMade = true; }
					if (custSettings.CustomDaysBetweenSmsAlerts == null) { custSettings.CustomDaysBetweenSmsAlerts = defaultCustSettings.CustomDaysBetweenSmsAlerts; messages.Add($"Setting default value CustomDaysBetweenSmsAlerts to {custSettings.CustomDaysBetweenSmsAlerts} for customer: {customer.name}"); changesMade = true; }


					if (custSettings.SendSmsAlerts == null) { custSettings.SendSmsAlerts = defaultCustSettings.SendSmsAlerts; messages.Add($"Setting default value SendSmsAlerts to {custSettings.SendSmsAlerts} for customer: {customer.name}"); changesMade = true; }
					if (custSettings.SendTrashSmsAlerts == null) { custSettings.SendTrashSmsAlerts = defaultCustSettings.SendTrashSmsAlerts; messages.Add($"Setting default value SendTrashSmsAlerts to {custSettings.SendTrashSmsAlerts} for customer: {customer.name}"); changesMade = true; }
					if (custSettings.HideLastPaymentDetails == null) { custSettings.HideLastPaymentDetails = defaultCustSettings.HideLastPaymentDetails; messages.Add($"Setting default value HideLastPaymentDetails to {custSettings.HideLastPaymentDetails} for customer: {customer.name}"); changesMade = true; }
					if (custSettings.HideStatementUrl == null) { custSettings.HideStatementUrl = defaultCustSettings.HideStatementUrl; messages.Add($"Setting default value HideStatementUrl to {custSettings.HideStatementUrl} for customer: {customer.name}"); changesMade = true; }

					
					if (custSettings.StatementUrl == null) { custSettings.StatementUrl = defaultCustSettings.StatementUrl; messages.Add($"Setting default value StatementUrl to {custSettings.StatementUrl} for customer: {customer.name}"); changesMade = true; }

					if (custSettings.EvictonNoticeDate == null) { custSettings.EvictonNoticeDate = defaultCustSettings.EvictonNoticeDate; messages.Add($"Setting default value EvictonNoticeDate to {custSettings.EvictonNoticeDate} for customer: {customer.name}"); changesMade = true; }
					if (custSettings.EvictionCourtCaseNumber == null) { custSettings.EvictionCourtCaseNumber = defaultCustSettings.EvictionCourtCaseNumber; messages.Add($"Setting default value EvictionCourtCaseNumber to {custSettings.EvictionCourtCaseNumber} for customer: {customer.name}"); changesMade = true; }
					if (custSettings.EvictionCourtAssignedJudge == null) { custSettings.EvictionCourtAssignedJudge = defaultCustSettings.EvictionCourtAssignedJudge; messages.Add($"Setting default value EvictionCourtAssignedJudge to {custSettings.EvictionCourtAssignedJudge} for customer: {customer.name}"); changesMade = true; }

					if (changesMade)
					{
						_customerService.SaveUpdatedCustomerSettings(customer, custSettings, _restService);
						processedCsutomers.Add(customer);
					}
					
				}

				if (string.IsNullOrWhiteSpace(customer.city) || (!customer.name.ToUpper().Contains("XX.XX") && !customer.city.ToUpper().Contains("ROOM")))
				{
					var addressId = customer.name.SubstringUpToFirst('-');
					var address = _appSettings.MahayagAddresses.First(x => x.Id == addressId);

					var roomName = "";
					
					if((!customer.name.ToUpper().Contains("XX.XX") && customer.city != null && !customer.city.ToUpper().Contains("ROOM")))
					{
						var roomId = customer.name.Substring(addressId.Length + 1, customer.name.IndexOfAny(new char[] { '(', '[' }) - 1 - addressId.Length).Replace(".", "").Replace("xx", "");

						roomName = " - " +
									roomId
									.Replace("F", "Floor ")
									.Replace("B", "Basement Floor ")
									.Replace("E", " East ")
									.Replace("W", " West ")
									.Replace("S", " South ")
									.Replace("N", " North ")
									+ $" Room ({roomId})"
								.Replace("   ", " ")
								.Replace("  ", " ")
								.Replace("   ", " ")
								.Replace("  ", " ")
								;
					}
					
					

					customer.city = $"{address.Address1}{roomName}, {address.City} {address.State} {address.ZipCode}";

					var updatedCustomerResult =
						_restService.Patch<UpdateCustomerResult, Customer>(customer.url, customer);

					if (updatedCustomerResult.IsSuccessStatusCode == false)
					{
						throw new InvalidOperationException("Set customer.city failed");
					}

					processedCsutomers.Add(customer);
					messages.Add($"Updated customer.city = {customer.city} for customer: {customer.name}");
				}
			//});
			}

			ViewBag.Message = string.Join(Environment.NewLine, messages); 
			return View(processedCsutomers);
		}

		public ActionResult ConsolidateInvoices(int narrowByCustomerId = 0)
		{
			var processedInvoices = new List<Invoice>();
			ViewBag.Message = "ConsolidateInvoices\r\n";

			var url = $"https://api.waveapps.com/businesses/{_appSettings.MahayagBusinessGuid}/invoices/?page_size=10000&embed_customer=true&embed_items=true";

			if (narrowByCustomerId != 0)
				url = url + $"&customer.id={narrowByCustomerId}";

			var invoicesDue = _restService.Get<List<Invoice>>(url).Result
				.Where(x=> x.invoice_amount_due != 0)
				.Where(x=> !x.customer.name.ToUpper().StartsWith("XXXX"))
				.OrderBy(x=>x.customer.name)
				.ToList();

			var distinctCustomerIds = invoicesDue.Select(x => x.customer.id).Distinct();

			foreach (var customerId in distinctCustomerIds)
			{
				var customerInvoicesDue = _restService.Get<List<Invoice>>($"https://api.waveapps.com/businesses/{_appSettings.MahayagBusinessGuid}/invoices/?page_size=10000&embed_customer=true&embed_items=true&customer.id={customerId}").Result
					.Where(x => x.invoice_amount_due != 0)
					.OrderByDescending(x => x.invoice_date)
					.ToList();

				var customer = customerInvoicesDue.First().customer;
				var custSettings = _customerService.ExctractSettingsFromCustomerObject(customer);

				try
				{ 
					//consolidate more than 2 invoices when settings allow it
					if (customerInvoicesDue.Count() > 1 && custSettings.ConsolidateInvoices == true && !customerInvoicesDue.First().customer.name.StartsWith("XXX"))
					{

						var target = customerInvoicesDue.Last();
						var sourceInvoices = customerInvoicesDue.Where(x => x.id != target.id);

						var targetInvoice = _restService.Get<Invoice>(target.url + "?&embed_items=true").Result;

						//foreach (var sourceInv in sourceInvoices.Where(w => w.disable_bank_payments && w.disable_credit_card_payments))
						foreach (var sourceInv in sourceInvoices)
						{
							var sourceInvoice = _restService.Get<Invoice>(sourceInv.url + "?&embed_items=true").Result;

							//check for payments
							var payments = _restService.Get<List<Payment>>(sourceInvoice.payments_url).Result;
							if (payments.Count == 0)
							{
								//process src invoice only if there are no payments
								#region transfer Items
								var sourceItems = sourceInvoice.items;

								decimal amountAdded = 0;
								string itemsAdded = "";

								foreach (var sourceItem in sourceItems.Where(x=> x.quantity * x.price != 0))
								{
									var product = _restService.Get<Product>(sourceItem.product.url).Result;
									product = product?? new Product {id = sourceItem.product.id};

									var movedItem = new InvoiceItem
									{
										product = product,
										description = $"Transfered item: [{sourceItem.description}] " +
										              $"from invoice: {sourceInvoice.invoice_number} " +
										              $"for period from: {sourceInvoice.invoice_date.ToUSADateFormat()} to: {sourceInvoice.due_date.ToUSADateFormat()} " +
										              $"transfered on: {DateTime.Now.ToUSADateFormat()}",
										quantity = sourceItem.quantity,
										price = sourceItem.price
									};

									targetInvoice.items.Add(movedItem);
									targetInvoice.invoice_amount_due += (movedItem.price * movedItem.quantity);

									itemsAdded += $"[{product?.name.Trim()} : {sourceItem.description.Trim()}; {(movedItem.price * movedItem.quantity).ToCurrency()}],";
									amountAdded += (movedItem.price * movedItem.quantity);

									if (UpdateInvoiceItems(targetInvoice))
									{

										#region zero out source item price

										sourceItem.description = $"[{sourceItem.description}] " +
										                         $"price: {movedItem.price.ToCurrency()} " +
										                         $"was moved to invoice: {targetInvoice.invoice_number} " +
										                         $"on: {DateTime.Now.ToUSADateFormat()}";
										sourceItem.price = 0;

										

										if (UpdateInvoiceItems(sourceInvoice) == false)
										{
											throw new InvalidOperationException("Saving zeroSourceInvoiceItem failed");
										}

										#endregion
									}
									else
									{
										throw new InvalidOperationException("add movedItem failed");
									}
								}
								#endregion

								processedInvoices.Add(sourceInvoice);

								if (custSettings.SendSmsAlerts == true && customer.date_created.Date <= DateTime.Today.Date.AddDays(-5))
								{
									#region sms alert customers for new invoice. 

									var statement = _restService.Get<TransactionHistory>($"https://api.waveapps.com/businesses/{_appSettings.MahayagBusinessGuid}" +
										$"/customers/{customer.id}/statements/transaction-history/?embed_items=true").Result;

									//sent alert to name 1
									if (!string.IsNullOrWhiteSpace(ExtractEmailFromString(customer.address1)))
									{
										
										var name = customer.first_name.ToUpper().Trim();
										var body = GetNewlyAddedConsolidatedInvoiceSmsAlertBody(name, sourceInv, targetInvoice, custSettings, amountAdded, itemsAdded, statement, "consolidated");

										//messages.Add($"alerting late customer:{name} on {ExtractEmailFromString(customer.address1)}");
										_sendGmail.SendSMS(ExtractEmailFromString(customer.address1), body, _appSettings.GoogleSettings);
									}

									//sent alert to name 2
									if (!string.IsNullOrWhiteSpace(ExtractEmailFromString(customer.address2)))
									{
										var name = customer.last_name.ToUpper().Trim();
										var body = GetNewlyAddedConsolidatedInvoiceSmsAlertBody(name, sourceInv, targetInvoice, custSettings, amountAdded, itemsAdded, statement, "consolidated");

										//messages.Add($"alerting late customer: {name} on {ExtractEmailFromString(customer.address2)}");
										_sendGmail.SendSMS(ExtractEmailFromString(customer.address2), body,
											_appSettings.GoogleSettings);
									}

									#endregion

									custSettings.LastSmsAlertSent = DateTime.Now;
									_customerService.SaveUpdatedCustomerSettings(customer, custSettings, _restService);
								}
							}
							else
							{
								//TODO: consolidate invoices with payments
							}
						}
					}

				}
				catch (Exception ex)
				{
					ViewBag.Message += $"\r\nConsolidate Invoice failed for customer: {customer.name} error: {ex.Message}";
					ViewBag.Message += $"\r\n";
				}

				
				
			}
			return View(processedInvoices);
		}


		public ActionResult RemindTrashDay(string propertyId = "0000", bool? recycle = null)
		{
			ViewBag.Message = "RemindTrashDay\r\n";

			var activeCustomers = GetActiveCustomers();

			foreach (var customer in activeCustomers.Where(x => x.name.StartsWith(propertyId)))
			{

				var custSettings = _customerService.ExctractSettingsFromCustomerObject(customer);

				try
				{
					if (custSettings.SendSmsAlerts == true && 
						custSettings.SendTrashSmsAlerts == true && 
						DateTime.Now.TimeOfDay >= TimeSpan.FromHours(8) && 
						custSettings.LastTrashSmsAlertSent < DateTime.Today)
					{
						#region sms alert customers for trash pickup.

						//sent alert to name 1
						if (!string.IsNullOrWhiteSpace(ExtractEmailFromString(customer.address1)))
						{

							var name = customer.first_name.ToUpper().Trim();
							var body = GetTrashSmsAlertBody(name, recycle);

							ViewBag.Message += $"alerting trash for customer:{name} on {ExtractEmailFromString(customer.address1)}\r\n";
							_sendGmail.SendSMS(ExtractEmailFromString(customer.address1), body, _appSettings.GoogleSettings);
						}

						//sent alert to name 2
						if (!string.IsNullOrWhiteSpace(ExtractEmailFromString(customer.address2)))
						{
							var name = customer.last_name.ToUpper().Trim();
							var body = GetTrashSmsAlertBody(name, recycle);

							ViewBag.Message += $"alerting trash for customer:{name} on {ExtractEmailFromString(customer.address2)}\r\n";
							_sendGmail.SendSMS(ExtractEmailFromString(customer.address2), body, _appSettings.GoogleSettings);
						}

						#endregion

						custSettings.LastTrashSmsAlertSent = DateTime.Now;
						_customerService.SaveUpdatedCustomerSettings(customer, custSettings, _restService);
					}
					else
					{
						ViewBag.Message += $"skipping trash for customer:{customer.name.Substring(0,10)} because " +
						                   $"SendSmsAlerts: {(custSettings.SendSmsAlerts == true ? "true" : "false")}" +
						                   $"TimeOfDay: {DateTime.Now.TimeOfDay}" +
						                   $"LastTrashSmsAlertSent: {custSettings.LastTrashSmsAlertSent}" +
						                   $"\r\n";

					}


				}
				catch (Exception ex)
				{
					ViewBag.Message += $"\r\nRemind Trash Day failed for customer: {customer.name} error: {ex.Message}";
					ViewBag.Message += $"\r\n";
				}



			}
			return View();
		}


		public ActionResult DisableInvoicePayments()
		{
			var invoices = _restService.Get<List<Invoice>>(
				$"https://api.waveapps.com/businesses/{_appSettings.MahayagBusinessGuid}/invoices/?page_size=10000&embed_customer=true&embed_items=true");

			var updatedInvoices = new ConcurrentBag<Invoice>();

			var messages = new ConcurrentBag<string>();

			if (invoices.IsSuccessStatusCode)
			{
				var invoicesToProcess = invoices.Result.Where(
					x => x.invoice_amount_due > 0 && 
					(x.disable_bank_payments == false || x.disable_credit_card_payments == false) &&
					!x.customer.name.ToUpper().StartsWith("XXXX")
				);

				foreach (var invoice in invoicesToProcess)
				{
					var disablePaymentInvoice = new InvoiceDisablePayments
					{
						disable_credit_card_payments = true,
						disable_bank_payments = true
					};

					var updatedInvoiceResult = _restService.Patch<Invoice, InvoiceDisablePayments>(invoice.url, disablePaymentInvoice);
					if (updatedInvoiceResult.IsSuccessStatusCode)
					{
						updatedInvoices.Add(updatedInvoiceResult.Result);
						messages.Add($"Disabled payments on invoice_number: {invoice.invoice_number}");
					}
					else
					{
						throw new InvalidOperationException("Failed to save disable_payments = true");
					}

					var disabledInvoice = _restService.Get<Invoice>(invoice.url + "?&embed_items=true&embed_customer=true&embed_product=true").Result;

					decimal amountAdded = 0;
					string itemsAdded = "";
					var custSettings = _customerService.ExctractSettingsFromCustomerObject(invoice.customer);

					foreach (var invoiceItem in disabledInvoice.items)
					{
						var product = _restService.Get<Product>(invoiceItem.product.url).Result;
						product = product ?? new Product { id = invoiceItem.product.id };

						itemsAdded += $"[{product?.name.Trim()} : {invoiceItem.description.Trim()}; {(invoiceItem.price * invoiceItem.quantity).ToCurrency()}],";
						amountAdded += (invoiceItem.price * invoiceItem.quantity);
					}

					if (custSettings.SendSmsAlerts == true && disabledInvoice.customer.date_created.Date <= DateTime.Today.Date.AddDays(-5))
					{
						#region sms alert customers for new invoice. 


						var statement = GetStatement(invoice.customer.id);

						//sent alert to name 1
						if (!string.IsNullOrWhiteSpace(ExtractEmailFromString(invoice.customer.address1)))
						{
							var name = invoice.customer.first_name.ToUpper().Trim();
							var body = GetNewlyAddedConsolidatedInvoiceSmsAlertBody(name, invoice, invoice, custSettings, amountAdded, itemsAdded, statement, "newly created");

							//messages.Add($"alerting late customer:{name} on {ExtractEmailFromString(customer.address1)}");
							_sendGmail.SendSMS(ExtractEmailFromString(invoice.customer.address1), body, _appSettings.GoogleSettings);
						}

						//sent alert to name 2
						if (!string.IsNullOrWhiteSpace(ExtractEmailFromString(invoice.customer.address2)))
						{
							var name = invoice.customer.last_name.ToUpper().Trim();
							var body = GetNewlyAddedConsolidatedInvoiceSmsAlertBody(name, invoice, invoice, custSettings, amountAdded, itemsAdded, statement, "newly created");

							//messages.Add($"alerting late customer: {name} on {ExtractEmailFromString(customer.address2)}");
							_sendGmail.SendSMS(ExtractEmailFromString(invoice.customer.address2), body, _appSettings.GoogleSettings);
						}
						#endregion

						custSettings.LastSmsAlertSent = DateTime.Now;
						_customerService.SaveUpdatedCustomerSettings(invoice.customer, custSettings, _restService);
					}
				};
			}
			else
			{
				throw new InvalidOperationException("Failed to retrieve invoice list");
			}

			ViewBag.Message = string.Join(Environment.NewLine, messages);
			return View(updatedInvoices);
		}

		public ActionResult ChargeLateFees(double lateRate = 0.02)
		{
			var defaultLatePercentRate = new decimal(lateRate);
			var catchupOnLatefees = false;
			List<Invoice> addedFeeInvoices = new List<Invoice>();

			var overdueInvoices = _restService.Get<List<Invoice>>(
				$"https://api.waveapps.com/businesses/{_appSettings.MahayagBusinessGuid}/invoices/?page_size=10000&status=overdue&embed_customer=true&embed_items=true");
			

			//TODO: handle multiple invoices per single customer
			foreach (var invoice in overdueInvoices.Result)
			{
				
				var custSettings = _customerService.ExctractSettingsFromCustomerObject(invoice.customer);

				if (custSettings?.ChargeLateFee != null && custSettings.ChargeLateFee.Value && 
					custSettings.NextLateFeeChargeDate != null && custSettings.NextLateFeeChargeDate.Value.Date <= DateTime.Now.Date 
				    && custSettings.LateFeeChargeAboveBalance <= invoice.invoice_amount_due && !invoice.customer.name.StartsWith("XXXX"))
				{
					#region add late fee
					var lateFee = new InvoiceItem
					{
						product = new Product{ id = (ulong)_appSettings.MahayagLateFeeProductId },
						description = $"Late Charge: {100 * (custSettings.LateFeePercentRate?? defaultLatePercentRate)}% " +
						              $"from PastDueAmount: {invoice.invoice_amount_due} " +
						              $"as of date: {custSettings.NextLateFeeChargeDate.Value.Date.ToUSADateFormat()} " +
						              $"added on: {DateTime.Now.ToUSADateFormat()}",
						quantity = 1,
						price = invoice.invoice_amount_due * (custSettings.LateFeePercentRate ?? defaultLatePercentRate)
					};

					var invoiceToAdd = _restService.Get<Invoice>(invoice.url + "?embed_customer=true&embed_items=true").Result;
					invoiceToAdd.items.Add(lateFee);

					if (UpdateInvoiceItems(invoiceToAdd))
					{
						addedFeeInvoices.Add(invoiceToAdd);

						#region update NextLateFeeChargeDate

						custSettings.NextLateFeeChargeDate = custSettings.NextLateFeeChargeDate.Value.AddDays(1).Date;

						var updatedCustomerResult = _customerService.SaveUpdatedCustomerSettings(invoice.customer, custSettings, _restService);

						if (updatedCustomerResult.IsSuccessStatusCode == false)
						{
							throw new InvalidOperationException("Saving NextLateFeeChargeDate failed");
						}

						#endregion
					}
					else
					{
						throw new InvalidOperationException("add lateFee failed");
					}
					#endregion
					
					//determine if process needs to catch up on late fees
					if (custSettings.NextLateFeeChargeDate.Value <= DateTime.Today)
						catchupOnLatefees = true;
				}
			}

			if (catchupOnLatefees)
			{
				//recusrsive call to catch up on late fees if process has not run for more than a day
				ChargeLateFees((double)defaultLatePercentRate);
			}

			return View(addedFeeInvoices);
		}



		private List<Tennant> GetTennatsFromName(string customerName)
		{
			List<Tennant> tenants = new List<Tennant>();

			int start = 0;
			int end = 0;

			do
			{
				start = customerName.IndexOf('[', start + 1);
				end = customerName.IndexOf(']', end + 1);

				var tenant = new Tennant()
				{
					FullName = customerName.Substring(start + 1, (end - start) - 11).ToUpper().Trim(),
					DateOfBirth = Convert.ToDateTime(customerName.Substring(end - 10, 10))
				};

				var names = tenant.FullName.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

				tenant.FirstName = names.First().ToUpper().Trim();
				tenant.LastName = names.Last().ToUpper().Trim();
				tenant.MiddleName = names.Count > 2 ? names[1].ToUpper().Trim() : string.Empty;

				tenants.Add(tenant);

			} while (customerName.IndexOf('[', end + 1) > 0);

			return tenants;
		}

		private string GetLateCustomerSmsAlertBody(string name, KeyValuePair<Customer, Transaction_History> customerKvp, Event lastPayment, Invoice lastInvoice, CustomerSettings custSettings)
		{
			var dailyRate = custSettings.SignedLeaseAgreement == true ? $"${custSettings.LateFeeDailyAmount}" : $"{custSettings.LateFeePercentRate * 100}%";
			var lastPmt = (lastPayment != null ? (lastPayment.date != null ? lastPayment.date.Value.ToUSADateFormat() : string.Empty) : string.Empty);
			var pmtUrl = "http://www.mahayagcbb.com/payment.html";
			var rentHelpUrl = "http://www.mahayagcbb.com/rent-help.html";
			var lastPaymentText = (custSettings.HideLastPaymentDetails != null && custSettings.HideLastPaymentDetails == true)
				?
					string.Empty
				:
					$"and your last payment of: ${lastPayment?.total} " +
					$"was accounted/credited on: {lastPmt}"
				;

			var statementUrlText = (custSettings.HideStatementUrl != null && custSettings.HideStatementUrl == true)
				?
					string.Empty
				:
					$"Please double check your payment history and remaining balance here: {custSettings.StatementUrl} "
				;

			 
			return  $"Hello {name}, " +
					$"as of today {DateTime.Today.ToUSADateFormat()} " +
					$"your balance due is ${customerKvp.Value.ending_balance} " +
					$"{lastPaymentText} ." +
					//$"Please double check your new/consolidated invoice remaining balance and payment(s) here: {lastInvoice.pdf_url.Replace("?pdf=1","")} ." +
					$"{statementUrlText} ." +
					$"If you made a payment in the last 2 to 4 business days it may still not be accounted for so please disregard this message . " +
					$"If you are ready to make a Payment please follow the instructions here {pmtUrl} . "+
					$"If you are struggling please look into for possible assistance resources here: {rentHelpUrl} ." +
					$"IMPORTANT: You must reply to this message every time and let me know when will you make your next payment or else I will pursue eviction. " +
					$"Delinquent accounts are subject to: {dailyRate}; daily charge for any past due balance! ";
		}

		private string GetNewlyAddedConsolidatedInvoiceSmsAlertBody(string name, Invoice sourceInv, Invoice targetInvoice, CustomerSettings custSettings, decimal amountAdded, string itemsAdded, TransactionHistory statement, string action)
		{
			return  $"Hello {name}, " +
					$"today {DateTime.Today.ToUSADateFormat()} " +
					$"a new amount of {amountAdded.ToCurrency()} " +
					$"was {action} " +
					$"to cover the item(s): {itemsAdded} " +
					$"for the period from: {sourceInv.invoice_date.ToUSADateFormat()} to: {sourceInv.due_date.ToUSADateFormat()} " +
					$"resulting of remaining balance due: {statement.transaction_history.FirstOrDefault()?.ending_balance.ToCurrency()}. " +
					$"Please double check your single invoice balance and payments here: {targetInvoice.pdf_url.Replace("?pdf=1", "")} ." +
					$"You can see your entire history here: {custSettings.StatementUrl} . " +
					$"IMPORTANT: You must reply to this message and let me know when and how will bring your balance to $0 if above. ";
		}


		private string GetTrashSmsAlertBody(string name, bool? recycle = null)
		{
			string recycleText = null;
			if (recycle == true)
				recycleText = "And recycle cans";

			return $"Hello {name}, " +
			       $"today is {DateTime.Today.ToUSADateFormat()} " +
				   $"and Tomorrow is trash day so if you could Please make sure the trash cans {recycleText} are pushed to the curb tonight so they can be collected tomorrow at 7am." +
			       $"Thank you and let me know if it is done. ";
		}


		private string GetAutoPinResetAndTextSmsAlertBody(string name, string oldPin, string newPin)
		{
			return $"Hello {name}, " +
			       $"your old pin code: {oldPin}; " +
			       $"will be reset and changed today around 3pm or later to " +
			       $"new pin code: {newPin}; " +
			       $"in order to prevent stale codes and to improve security. " +
			       $"Please be advised only one code will be active at the same time per tenant. " +
			       $"IMPORTANT NOTE: Do not share your pin code with anyone for any reason !!! ";
		}

		private Dictionary<Customer, Transaction_History> GetLateCustomers(ulong narrowByCustomerId = 0)
		{
			var toReturn = new Dictionary<Customer, Transaction_History>();

			if (memoryCache == null)
				memoryCache = new MemoryCache(new MemoryCacheOptions
				{

				});

			

			var activeCustomers = GetActiveCustomers();
			if (narrowByCustomerId > 0)
				activeCustomers = activeCustomers.Where(x => x.id == narrowByCustomerId).ToList();

			var allCustomerStatements = new Dictionary<Customer, Transaction_History>();

			Parallel.ForEach(activeCustomers, (customer) =>
			//foreach (var customer in activeCustomers)
			{

				//var statement = _restService.Get<TransactionHistory>($"https://api.waveapps.com/businesses/{_appSettings.MahayagBusinessGuid}/customers/{customer.id}/statements/transaction-history/").Result;

				//var trxHistory = statement.transaction_history.FirstOrDefault();

				string serializedObject = "";
				Transaction_History trxHistory = null;
				var key = $"Transaction_History-{customer.id}";

				try
				{
					if (memoryCache.TryGetValue(key, out serializedObject))
					{
						trxHistory = JsonConvert.DeserializeObject<Transaction_History>(serializedObject);
					}
					else
					{
						trxHistory = GetStatementTrxHistory(customer.id);


						memoryCache.Set(
							key,
							JsonConvert.SerializeObject(trxHistory),
							new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(43200))
						);
					}

					allCustomerStatements.Add(customer, trxHistory);
				}
				catch (Exception ex)
				{

				}
			//}
			})
			;

			foreach (var keyValuePair in allCustomerStatements/*.Where(x => x.Value.ending_balance > 0)*/.OrderByDescending(x => x.Value.ending_balance))
			{
				toReturn.Add(keyValuePair.Key, keyValuePair.Value);
			}


				
			

			return toReturn;
		}

		private TransactionHistory GetStatement(ulong customerId, bool getItems = false)
		{
			var stHistory = GetStatementTrxHistory(customerId, getItems);
			return new TransactionHistory {transaction_history = new List<Transaction_History>{ stHistory } };
		}

		private Transaction_History GetStatementTrxHistory(ulong customerId, bool getItems = false)
		{
			var trxHistory = new Transaction_History { events = new List<Event>() };

			var allCustInvoices = new List<Invoice>();

			int count = 0;
			int page = 1;
			do
			{
				var list = _restService.Get<List<Invoice>>
					($"https://api.waveapps.com/businesses/{_appSettings.MahayagBusinessGuid}/invoices/?page_size=50&page={page}&customer.id={customerId}")
				.Result;

				allCustInvoices.AddRange(list);

				page++;
				count = list.Count;

				if (page > 50)
					break;
			}
			while (count == 50);

			var allCustInvPayments = new List<Payment>();

			//Parallel.ForEach(allCustInvoices.Where(i=>i.invoice_amount_paid != 0), inv =>
			foreach (var inv in allCustInvoices)
			{
				var invPayments = _restService.Get<List<Payment>>(inv.payments_url).Result;

				foreach (var pm in invPayments)
				{
					allCustInvPayments.Add(pm);
				}

			}
			//})
			;

			//Parallel.ForEach(allCustInvoices, inv =>
			foreach (var inv in allCustInvoices)
			{
				trxHistory.events.Add(new Event
				{
					date = DateTime.Parse(inv.invoice_date),
					event_type = "invoice",
					total = inv.invoice_total,
					invoice = inv
				});

				if (getItems == true)
				{
					var fullInvoice = _restService.Get<Invoice>
						($"https://api.waveapps.com/businesses/{_appSettings.MahayagBusinessGuid}/invoices/{inv.id}/?embed_accounts=true&embed_customer=true&embed_discounts=true&embed_deposits=true&embed_items=true&embed_payments=true&embed_products=true&embed_sales_taxes=true")
						.Result;

					inv.items = fullInvoice.items;
				}

			}
			//})
			;

			var separatePayments = new List<Event> ();

			foreach (var pmt in allCustInvPayments)
			{
				DateTime? creditonDifferentDate = null;
				if (pmt.memo.Contains("creditDate"))
				{
					//var memoObject = JsonConvert.DeserializeObject(pmt.memo);
				}
				separatePayments.Add(new Event
				{
					date = creditonDifferentDate ?? DateTime.Parse(pmt.payment_date),
					event_type = "payment",
					total = pmt.amount,
					//payment = pmt
				});
			}

			var aggregatePayments = from sp in separatePayments
									group sp by sp.date into p
									select new Event
									{
										date = p.First().date,
										event_type = p.First().event_type,
										total = p.Sum(s=>s.total),
									};

			foreach(var p in aggregatePayments)
			{
				if(p == null)
				{

				}
				else
					trxHistory.events.Add(p);
			}

			trxHistory.ending_balance = allCustInvoices.Sum(s => s.invoice_amount_due);

			return trxHistory;
		}

		private List<Customer> GetActiveCustomers()
		{
			var allCustomers = _restService.Get<List<Customer>>(
				$"https://api.waveapps.com/businesses/{_appSettings.MahayagBusinessGuid}/customers/").Result;

			var activeCustomers = allCustomers.Where(x => x.active && !x.name.StartsWith("XX")).ToList();

			return activeCustomers;
		}

		private bool UpdateInvoiceItems(Invoice invoice)
		{
			//var itemsOnly = new InvoiceItemsOnly
			//{
			//	items = invoice.items,
			//	id = invoice.id,
			//};

			//invoice.source_invoice_number = "1";

			var result = _restService.Put(invoice.url, invoice);

			if (result.IsSuccessStatusCode)
				return true;

			ViewBag.Message += $"\r\nUpdateInvoiceItems failed for invoice: {invoice.invoice_number} with error: {result.ErrorMessage}";

			return false;
		}

		private class UpdateInvoiceItemResponse
		{
		}

		private class UpdateCustomerResult
		{
		}
	}
}
