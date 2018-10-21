using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Models;
using Common.Services;
using WaveAccountingIntegration.Models;

namespace WaveAccountingIntegration.Services
{
	public interface ICustomerService
	{
		CustomerSettings ExctractSettingsFromCustomerObject(Customer customer);

		RestResult<Customer> SaveUpdatedCustomerSettings(Customer customer, CustomerSettings updatedSettings, IRestService _restService);
		RestResult<Customer> SaveUpdatedCustomerPhone(Customer customer, string  updatedPhone, IRestService _restService);
	}
}
