using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveAccountingIntegration.Models;

namespace WaveAccountingIntegration.Services
{
	public interface ICustomerSettingsService
	{
		CustomerSettings ExctractFromCustomerObject(Customer customer);

		RestResult<Customer> SaveUpdatedCustomerSettings(string customerUrl, CustomerSettings updatedSettings, IRestService _restService);
	}
}
