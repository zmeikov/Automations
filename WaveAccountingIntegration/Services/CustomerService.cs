using System;
using Common.Models;
using Common.Services;
using Newtonsoft.Json;
using WaveAccountingIntegration.Models;

namespace WaveAccountingIntegration.Services
{
	public class CustomerService : ICustomerService
	{
		public CustomerSettings ExctractSettingsFromCustomerObject(Customer customer)
		{
			if (customer.shipping_details?.delivery_instructions != null)
			{
				var custSettings = JsonConvert.DeserializeObject<CustomerSettings>(customer.shipping_details.delivery_instructions);

				return custSettings;
			}
			return null;
		}

		public RestResult<Customer> SaveUpdatedCustomerSettings(Customer customer, CustomerSettings updatedSettings, IRestService _restService)
		{
			var updatedCustomerSettings = new CustomerUpdateSettings
			{
				shipping_details = new CustomerUpdateSettings.Update_Shipping_Details
				{
					ship_to_contact = customer.shipping_details.ship_to_contact,
					delivery_instructions = JsonConvert.SerializeObject(updatedSettings)
				}
			};

			var updatedCustomerResult = _restService.Patch<Customer, CustomerUpdateSettings>(customer.url, updatedCustomerSettings);

			return updatedCustomerResult;
		}

		public RestResult<Customer> SaveUpdatedCustomerPhone(Customer customer, string updatedPhone, IRestService _restService)
		{
			var updatedCustomerPhone = new CustomerUpdatePhone
			{
				phone_number = updatedPhone
			};

			var updatedCustomerResult = _restService.Patch<Customer, CustomerUpdatePhone>(customer.url, updatedCustomerPhone);

			return updatedCustomerResult;
		}
	}
}