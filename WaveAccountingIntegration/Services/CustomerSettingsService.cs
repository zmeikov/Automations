using System;
using Newtonsoft.Json;
using WaveAccountingIntegration.Models;

namespace WaveAccountingIntegration.Services
{
	public class CustomerSettingsService : ICustomerSettingsService
	{
		public CustomerSettings ExctractFromCustomerObject(Customer customer)
		{
			if (customer.shipping_details?.delivery_instructions != null)
			{
				var custSettings =
				JsonConvert.DeserializeObject<Models.CustomerSettings>(customer.shipping_details.delivery_instructions);

				return custSettings;
			}
			return null;
		}

		public RestResult<Customer> SaveUpdatedCustomerSettings(string customerUrl, CustomerSettings updatedSettings, IRestService _restService)
		{
			var updatedCustomerSettings = new CustomerUpdateSettings
			{
				shipping_details = new CustomerUpdateSettings.Update_Shipping_Details
				{
					delivery_instructions = JsonConvert.SerializeObject(updatedSettings)
				}
			};

			var updatedCustomerResult = _restService.Patch<Customer, CustomerUpdateSettings>(customerUrl, updatedCustomerSettings);

			return updatedCustomerResult;
		}
	}
}